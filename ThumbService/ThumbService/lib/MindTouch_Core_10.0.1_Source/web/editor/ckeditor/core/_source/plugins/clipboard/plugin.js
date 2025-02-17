﻿/*
Copyright (c) 2003-2010, CKSource - Frederico Knabben. All rights reserved.
For licensing, see LICENSE.html or http://ckeditor.com/license
*/

/**
 * @file Clipboard support
 */

(function()
{
	// Tries to execute any of the paste, cut or copy commands in IE. Returns a
	// boolean indicating that the operation succeeded.
	var execIECommand = function( editor, command )
	{
		var doc = editor.document,
			body = doc.getBody();

		var	enabled = false;
		var onExec = function()
		{
			enabled = true;
		};

		// The following seems to be the only reliable way to detect that
		// clipboard commands are enabled in IE. It will fire the
		// onpaste/oncut/oncopy events only if the security settings allowed
		// the command to execute.
		body.on( command, onExec );

		// IE6/7: document.execCommand has problem to paste into positioned element.
		( CKEDITOR.env.version > 7 ? doc.$ : doc.$.selection.createRange() ) [ 'execCommand' ]( command );

		body.removeListener( command, onExec );

		return enabled;
	};

	// Attempts to execute the Cut and Copy operations.
	var tryToCutCopy =
		CKEDITOR.env.ie ?
			function( editor, type )
			{
				return execIECommand( editor, type );
			}
		:		// !IE.
			function( editor, type )
			{
				try
				{
					// Other browsers throw an error if the command is disabled.
					return editor.document.$.execCommand( type );
				}
				catch( e )
				{
					return false;
				}
			};

	// A class that represents one of the cut or copy commands.
	var cutCopyCmd = function( type )
	{
		this.type = type;
		this.canUndo = ( this.type == 'cut' );		// We can't undo copy to clipboard.
	};

	cutCopyCmd.prototype =
	{
		exec : function( editor, data )
		{
			this.type == 'cut' && fixCut( editor );

			var success = tryToCutCopy( editor, this.type );

			if ( !success )
				alert( editor.lang.clipboard[ this.type + 'Error' ] );		// Show cutError or copyError.

			return success;
		}
	};

	// Paste command.
	var pasteCmd =
	{
		canUndo : false,

		exec :
			CKEDITOR.env.ie ?
				function( editor )
				{
					// Prevent IE from pasting at the begining of the document.
					editor.focus();

					if ( !editor.document.getBody().fire( 'beforepaste' )
						 && !execIECommand( editor, 'paste' ) )
					{
						editor.fire( 'pasteDialog' );
						return false;
					}
				}
			:
				function( editor )
				{
					try
					{
						if ( !editor.document.getBody().fire( 'beforepaste' )
							 && !editor.document.$.execCommand( 'Paste', false, null ) )
						{
							throw 0;
						}
					}
					catch ( e )
					{
						setTimeout( function()
							{
								editor.fire( 'pasteDialog' );
							}, 0 );
						return false;
					}
				}
	};

	// Listens for some clipboard related keystrokes, so they get customized.
	var onKey = function( event )
	{
		if ( this.mode != 'wysiwyg' )
			return;

		switch ( event.data.keyCode )
		{
			// Paste
			case CKEDITOR.CTRL + 86 :		// CTRL+V
			case CKEDITOR.SHIFT + 45 :		// SHIFT+INS

				var body = this.document.getBody();

				// Simulate 'beforepaste' event for all none-IEs.
				if ( !CKEDITOR.env.ie && body.fire( 'beforepaste' ) )
					event.cancel();
				// Simulate 'paste' event for Opera/Firefox2.
				else if ( CKEDITOR.env.opera
						 || CKEDITOR.env.gecko && CKEDITOR.env.version < 10900 )
					body.fire( 'paste' );
				return;

			// Cut
			case CKEDITOR.CTRL + 88 :		// CTRL+X
			case CKEDITOR.SHIFT + 46 :		// SHIFT+DEL

				// Save Undo snapshot.
				var editor = this;
				this.fire( 'saveSnapshot' );		// Save before paste
				setTimeout( function()
					{
						editor.fire( 'saveSnapshot' );		// Save after paste
					}, 0 );
		}
	};

	// Allow to peek clipboard content by redirecting the
	// pasting content into a temporary bin and grab the content of it.
	function getClipboardData( evt, mode, callback )
	{
		var doc = this.document;

		// Avoid recursions on 'paste' event for IE.
		if ( CKEDITOR.env.ie && doc.getById( 'cke_pastebin' ) )
			return;

		// If the browser supports it, get the data directly
		if (mode == 'text' && evt.data && evt.data.$.clipboardData)
		{
			// evt.data.$.clipboardData.types contains all the flavours in Mac's Safari, but not on windows.
			var plain = evt.data.$.clipboardData.getData( 'text/plain' );
			if (plain)
			{
				evt.data.preventDefault();
				callback( plain );
				return;
			}
		}

		var sel = this.getSelection(),
			range = new CKEDITOR.dom.range( doc );

		// Create container to paste into
		var pastebin = new CKEDITOR.dom.element( mode == 'text' ? 'textarea' : CKEDITOR.env.webkit ? 'body' : 'div', doc );
		pastebin.setAttribute( 'id', 'cke_pastebin' );
		/**
		 * WebKit splits the div into multiple ones
		 * @author MindTouch
		 */
		pastebin.setAttribute( 'class', 'cke_pastebin' );
		/* END */
		// Safari requires a filler node inside the div to have the content pasted into it. (#4882)
		CKEDITOR.env.webkit && pastebin.append( doc.createText( '\xa0' ) );
		doc.getBody().append( pastebin );

		/**
		 * getDocumentPosition() returns the top position of the element
		 * and in case if element contains too much contents
		 * cursor position does not match document position
		 * 
		 * @author MindTouch
		 */
		var docScrollY = doc.getWindow().getScrollPosition().y;
		var top = docScrollY;

		if ( docScrollY == 0 )
		{
			var contents = this.getThemeSpace( 'contents' );

			top = CKEDITOR.document.getWindow().getScrollPosition().y -
				contents.getDocumentPosition().y;

			if ( top < 0 )
			{
				top = sel.getStartElement().getDocumentPosition().y;
			}
		}

		pastebin.setStyles(
			{
				position : 'absolute',
				// Position the bin exactly at the position of the selected element
				// to avoid any subsequent document scroll.
				top : top + 'px',
//				top : sel.getStartElement().getDocumentPosition().y + 'px',
				width : '1px',
				height : '1px',
				overflow : 'hidden'
			});
		/* END */

		// It's definitely a better user experience if we make the paste-bin pretty unnoticed
		// by pulling it off the screen.
		pastebin.setStyle( this.config.contentsLangDirection == 'ltr' ? 'left' : 'right', '-1000px' );

		var bms = sel.createBookmarks();

		// Turn off design mode temporarily before give focus to the paste bin.
		if ( mode == 'text' )
		{
			if ( CKEDITOR.env.ie )
			{
				var ieRange = doc.getBody().$.createTextRange();
				ieRange.moveToElementText( pastebin.$ );
				ieRange.execCommand( 'Paste' );
				evt.data.preventDefault();
			}
			else
			{
				doc.$.designMode = 'off';
				pastebin.$.focus();
			}
		}
		else
		{
			range.setStartAt( pastebin, CKEDITOR.POSITION_AFTER_START );
			range.setEndAt( pastebin, CKEDITOR.POSITION_BEFORE_END );
			range.select( true );
		}

		// Wait a while and grab the pasted contents
		window.setTimeout( function()
		{
			mode == 'text' && !CKEDITOR.env.ie && ( doc.$.designMode = 'on' );

			/**
			 * @see #0007663: Pasting text into editor inserts divs
			 * @author MindTouch
			 */
			if ( mode == 'html' )
			{
				var nodeList = doc.getElementsByTag( pastebin.getName() ),
					pasteBins = [], i, bin;

				for ( i = 0 ; i < nodeList.count() ; i++ )
				{
					bin = nodeList.getItem( i );

					if ( bin.hasClass( 'cke_pastebin' ) )
					{
						pasteBins.push( bin );
					}
				}

				var binContents = '',
					br = new CKEDITOR.dom.element( 'br', doc );

				for ( i = 0 ; i < pasteBins.length ; i++ )
				{
					bin = pasteBins[ i ];
					bin.remove();

					// if there are multiple bins
					// the first node contains duplicates of all other nodes
					if ( CKEDITOR.env.webkit && pasteBins.length > 1 && i == 0 )
					{
						continue;
					}

					if ( CKEDITOR.env.webkit )
					{
						var $bin = Deki.$( bin.$ );

						// Grab the HTML contents.
						// We need to look for a apple style wrapper on webkit it also adds
						// a div wrapper if you copy/paste the body of the editor.
						// Remove hidden div and restore selection.
						$bin.find( 'span.Apple-style-span' ).each( function()
							{
								var span = new CKEDITOR.dom.element( this );
								span.remove( true );
							} );

						$bin.children( 'meta' ).remove();
					}

					binContents += bin.getHtml();

					if ( i < pasteBins.length - 1 )
					{
						binContents += br.getOuterHtml();
					}
				}

				pastebin.setHtml( binContents );
			}
			/* END */
			
			pastebin.remove();

			sel.selectBookmarks( bms );
			callback( pastebin[ 'get' + ( mode == 'text' ? 'Value' : 'Html' ) ]() );
		}, 0 );
	}

	// Cutting off control type element in IE standards breaks the selection entirely. (#4881)
	function fixCut( editor )
	{
		if ( !CKEDITOR.env.ie || editor.document.$.compatMode == 'BackCompat' )
			return;

		var sel = editor.getSelection();
		var control;
		if( ( sel.getType() == CKEDITOR.SELECTION_ELEMENT ) && ( control = sel.getSelectedElement() ) )
		{
			var range = sel.getRanges()[ 0 ];
			var dummy = editor.document.createText( '' );
			dummy.insertBefore( control );
			range.setStartBefore( dummy );
			range.setEndAfter( control );
			sel.selectRanges( [ range ] );

			// Clear up the fix if the paste wasn't succeeded.
			setTimeout( function()
			{
				// Element still online?
				if ( control.getParent() )
				{
					dummy.remove();
					sel.selectElement( control );
				}
			}, 0 );
		}
	}

	// Register the plugin.
	CKEDITOR.plugins.add( 'clipboard',
		{
			requires : [ 'dialog', 'htmldataprocessor' ],
			init : function( editor )
			{
				// Inserts processed data into the editor at the end of the
				// events chain.
				editor.on( 'paste', function( evt )
					{
						var data = evt.data;
						if ( data[ 'html' ] )
							editor.insertHtml( data[ 'html' ] );
						else if ( data[ 'text' ] )
							editor.insertText( data[ 'text' ] );

					}, null, null, 1000 );

				editor.on( 'pasteDialog', function( evt )
					{
						setTimeout( function()
						{
							// Open default paste dialog.
							editor.openDialog( 'paste' );
						}, 0 );
					});

				function addButtonCommand( buttonName, commandName, command, ctxMenuOrder )
				{
					var lang = editor.lang[ commandName ];

					editor.addCommand( commandName, command );
					editor.ui.addButton( buttonName,
						{
							label : lang,
							command : commandName
						});

					// If the "menu" plugin is loaded, register the menu item.
					if ( editor.addMenuItems )
					{
						editor.addMenuItem( commandName,
							{
								label : lang,
								command : commandName,
								group : 'clipboard',
								order : ctxMenuOrder
							});
					}
				}

				addButtonCommand( 'Cut', 'cut', new cutCopyCmd( 'cut' ), 1 );
				addButtonCommand( 'Copy', 'copy', new cutCopyCmd( 'copy' ), 4 );
				addButtonCommand( 'Paste', 'paste', pasteCmd, 8 );

				CKEDITOR.dialog.add( 'paste', CKEDITOR.getUrl( this.path + 'dialogs/paste.js' ) );

				editor.on( 'key', onKey, editor );

				// We'll be catching all pasted content in one line, regardless of whether the
				// it's introduced by a document command execution (e.g. toolbar buttons) or
				// user paste behaviors. (e.g. Ctrl-V)
				editor.on( 'contentDom', function()
				{
					var body = editor.document.getBody();
					var onPaste = function( evt )
					{
						if ( depressBeforeEvent )
							return;

						var mode = editor.config.forcePasteAsPlainText ? 'text' : 'html';

						/**
						 * Force pasting mode in pre.script blocks
						 *
						 * @see #0007029
						 * @author MindTouch
						 */
						var sel = editor.getSelection(),
							pre = sel && sel.getStartElement().getAscendant( 'pre', true );

						if ( pre && pre.hasClass( 'script' ) )
						{
							mode = 'text';
						}

						if ( CKEDITOR.env.ie )
						{
							if ( ( mode == 'text' && evt.name == 'beforepaste' ) ||
								 ( mode == 'html' && evt.name == 'paste' ) )
							{
								return;
							}
						}
						// Use gecko engine to pasting since ckeditor is buggy
						else if ( CKEDITOR.env.gecko && mode == 'text' )
						{
							window.setTimeout( function()
								{
									editor.fire( 'paste', {} );
								}, 0 );

							return;
						}
						/* END */

						getClipboardData.call( editor, evt, mode, function ( data )
						{
							// The very last guard to make sure the
							// paste has successfully happened.
							if ( !data )
								return;

							var dataTransfer = {};
							dataTransfer[ mode ] = data;
							editor.fire( 'paste', dataTransfer );
						} );
					};

					body.on( ( CKEDITOR.env.webkit ) ? 'paste' : 'beforepaste', onPaste );

					if ( CKEDITOR.env.ie )
					{
						body.on( 'paste', onPaste );
					}

					body.on( 'beforecut', function() { !depressBeforeEvent && fixCut( editor ); } );
				});

				// If the "contextmenu" plugin is loaded, register the listeners.
				if ( editor.contextMenu )
				{
					var depressBeforeEvent;
					function stateFromNamedCommand( command )
					{
						// IE Bug: queryCommandEnabled('paste') fires also 'beforepaste(copy/cut)',
						// guard to distinguish from the ordinary sources( either
						// keyboard paste or execCommand ) (#4874).
						CKEDITOR.env.ie && ( depressBeforeEvent = 1 );

						var retval = editor.document.$.queryCommandEnabled( command ) ? CKEDITOR.TRISTATE_OFF : CKEDITOR.TRISTATE_DISABLED;
						depressBeforeEvent = 0;
						return retval;
					}

					editor.contextMenu.addListener( function()
						{
							return {
								cut : stateFromNamedCommand( 'Cut' ),

								// Browser bug: 'Cut' has the correct states for both Copy and Cut.
								copy : stateFromNamedCommand( 'Cut' ),
								paste : CKEDITOR.env.webkit ? CKEDITOR.TRISTATE_OFF : stateFromNamedCommand( 'Paste' )
							};
						});
				}
			}
		});
})();

/**
 * Fired when a clipboard operation is about to be taken into the editor.
 * Listeners can manipulate the data to be pasted before having it effectively
 * inserted into the document.
 * @name CKEDITOR.editor#paste
 * @since 3.1
 * @event
 * @param {String} [data.html] The HTML data to be pasted. If not available, e.data.text will be defined.
 * @param {String} [data.text] The plain text data to be pasted, available when plain text operations are to used. If not available, e.data.html will be defined.
 */
