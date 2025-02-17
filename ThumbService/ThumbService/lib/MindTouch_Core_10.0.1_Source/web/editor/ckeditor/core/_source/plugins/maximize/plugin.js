﻿/*
Copyright (c) 2003-2010, CKSource - Frederico Knabben. All rights reserved.
For licensing, see LICENSE.html or http://ckeditor.com/license
*/

(function()
{
	function protectFormStyles( formElement )
	{
		if ( !formElement || formElement.type != CKEDITOR.NODE_ELEMENT || formElement.getName() != 'form' )
			return [];

		var hijackRecord = [];
		var hijackNames = [ 'style', 'className' ];
		for ( var i = 0 ; i < hijackNames.length ; i++ )
		{
			var name = hijackNames[i];
			var $node = formElement.$.elements.namedItem( name );
			if ( $node )
			{
				var hijackNode = new CKEDITOR.dom.element( $node );
				hijackRecord.push( [ hijackNode, hijackNode.nextSibling ] );
				hijackNode.remove();
			}
		}

		return hijackRecord;
	}

	function restoreFormStyles( formElement, hijackRecord )
	{
		if ( !formElement || formElement.type != CKEDITOR.NODE_ELEMENT || formElement.getName() != 'form' )
			return;

		if ( hijackRecord.length > 0 )
		{
			for ( var i = hijackRecord.length - 1 ; i >= 0 ; i-- )
			{
				var node = hijackRecord[i][0];
				var sibling = hijackRecord[i][1];
				if ( sibling )
					node.insertBefore( sibling );
				else
					node.appendTo( formElement );
			}
		}
	}

	function saveStyles( element, isInsideEditor )
	{
		var data = protectFormStyles( element );
		var retval = {};

		var $element = element.$;

		if ( !isInsideEditor )
		{
			retval[ 'class' ] = $element.className || '';
			$element.className = '';
		}

		retval.inline = $element.style.cssText || '';
		if ( !isInsideEditor )		// Reset any external styles that might interfere. (#2474)
			$element.style.cssText = 'position: static; overflow: visible';

		restoreFormStyles( data );
		return retval;
	}

	function restoreStyles( element, savedStyles )
	{
		var data = protectFormStyles( element );
		var $element = element.$;
		if ( 'class' in savedStyles )
			$element.className = savedStyles[ 'class' ];
		if ( 'inline' in savedStyles )
			$element.style.cssText = savedStyles.inline;
		restoreFormStyles( data );
	}

	function refreshCursor( editor )
	{
		// Refresh all editor instances on the page (#5724).
		var all = CKEDITOR.instances;
		for ( var i in all )
		{
			var one = all[ i ];
			if ( one.mode == 'wysiwyg' )
			{
				var body = one.document.getBody();
				// Refresh 'contentEditable' otherwise
				// DOM lifting breaks design mode. (#5560)
				body.setAttribute( 'contentEditable', false );
				body.setAttribute( 'contentEditable', true );
			}
		}

		if ( editor.focusManager.hasFocus )
		{
			editor.toolbox.focus();
			editor.focus();
		}
	}

	/**
	 * Adding an iframe shim to this element, OR removing the existing one if already applied.
	 * Note: This will only affect IE version below 7.
	 */
	 function createIframeShim( element )
	{
		if ( !CKEDITOR.env.ie || CKEDITOR.env.version > 6 )
			return null;

		var shim = CKEDITOR.dom.element.createFromHtml( '<iframe frameborder="0" tabindex="-1"' +
					' src="javascript:' +
					   'void((function(){' +
						   'document.open();' +
						   ( CKEDITOR.env.isCustomDomain() ? 'document.domain=\'' + this.getDocument().$.domain + '\';' : '' ) +
						   'document.close();' +
					   '})())"' +
					' style="display:block;position:absolute;z-index:-1;' +
					'progid:DXImageTransform.Microsoft.Alpha(opacity=0);' +
					'"></iframe>' );
		return element.append( shim, true );
	}

	CKEDITOR.plugins.add( 'maximize',
	{
		init : function( editor )
		{
			var lang = editor.lang;
			var mainDocument = CKEDITOR.document;
			var mainWindow = mainDocument.getWindow();

			// Saved selection and scroll position for the editing area.
			var savedSelection;
			var savedScroll;

			// Saved scroll position for the outer window.
			var outerScroll;

			var shim;

			// Saved resize handler function.
			function resizeHandler()
			{
				var viewPaneSize = mainWindow.getViewPaneSize();
				shim && shim.setStyles( { width : viewPaneSize.width + 'px', height : viewPaneSize.height + 'px' } );
				editor.resize( viewPaneSize.width, viewPaneSize.height, null, true );
			}

			// Retain state after mode switches.
			var savedState = CKEDITOR.TRISTATE_OFF;

			editor.addCommand( 'maximize',
				{
					modes : { wysiwyg : 1, source : 1 },
					editorFocus : false,
					exec : function()
					{
						var container = editor.container.getChild( 1 );
						var contents = editor.getThemeSpace( 'contents' );

						// Save current selection and scroll position in editing area.
						if ( editor.mode == 'wysiwyg' )
						{
							var selection = editor.getSelection();
							savedSelection = selection && selection.getRanges();
							savedScroll = mainWindow.getScrollPosition();
						}
						else
						{
							var $textarea = editor.textarea.$;
							savedSelection = !CKEDITOR.env.ie && [ $textarea.selectionStart, $textarea.selectionEnd ];
							savedScroll = [ $textarea.scrollLeft, $textarea.scrollTop ];
						}

						if ( this.state == CKEDITOR.TRISTATE_OFF )		// Go fullscreen if the state is off.
						{
							// Add event handler for resizing.
							mainWindow.on( 'resize', resizeHandler );

							// Save the scroll bar position.
							outerScroll = mainWindow.getScrollPosition();

							// Save and reset the styles for the entire node tree.
							var currentNode = editor.container;
							while ( ( currentNode = currentNode.getParent() ) )
							{
								currentNode.setCustomData( 'maximize_saved_styles', saveStyles( currentNode ) );
								currentNode.setStyle( 'z-index', editor.config.baseFloatZIndex - 1 );
							}
							contents.setCustomData( 'maximize_saved_styles', saveStyles( contents, true ) );
							container.setCustomData( 'maximize_saved_styles', saveStyles( container, true ) );

							// Hide scroll bars.
							if ( CKEDITOR.env.ie )
							{
								mainDocument.$.documentElement.style.overflow =
									mainDocument.getBody().$.style.overflow = 'hidden';
							}
							else
							{
								/**
								 * Maximize command works incorrect in some cases
								 * 
								 * @author MindTouch
								 * @see #0008069
								 */
								mainDocument.getBody().setStyle( 'overflow', 'hidden' );
//								mainDocument.getBody().setStyles(
//									{
//										overflow : 'hidden',
//										width : '0px',
//										height : '0px'
//									} );
								/* END */
							}

							// Scroll to the top left (IE needs some time for it - #4923).
							CKEDITOR.env.ie ?
								setTimeout( function() { mainWindow.$.scrollTo( 0, 0 ); }, 0 ) :
								mainWindow.$.scrollTo( 0, 0 );

							// Resize and move to top left.
							var viewPaneSize = mainWindow.getViewPaneSize();
							container.setStyle( 'position', 'absolute' );
							container.$.offsetLeft;			// SAFARI BUG: See #2066.
							container.setStyles(
								{
									'z-index' : editor.config.baseFloatZIndex - 1,
									left : '0px',
									top : '0px'
								} );

							shim =  createIframeShim( container );		// IE6 select element penetration when maximized. (#4459)
							resizeHandler();

							// Still not top left? Fix it. (Bug #174)
							var offset = container.getDocumentPosition();
							container.setStyles(
								{
									left : ( -1 * offset.x ) + 'px',
									top : ( -1 * offset.y ) + 'px'
								} );

							// Fixing positioning editor chrome in Firefox break design mode. (#5149)
							CKEDITOR.env.gecko && refreshCursor( editor );

							// Add cke_maximized class.
							container.addClass( 'cke_maximized' );
						}
						else if ( this.state == CKEDITOR.TRISTATE_ON )	// Restore from fullscreen if the state is on.
						{
							// Remove event handler for resizing.
							mainWindow.removeListener( 'resize', resizeHandler );

							// Restore CSS styles for the entire node tree.
							var editorElements = [ contents, container ];
							for ( var i = 0 ; i < editorElements.length ; i++ )
							{
								restoreStyles( editorElements[i], editorElements[i].getCustomData( 'maximize_saved_styles' ) );
								editorElements[i].removeCustomData( 'maximize_saved_styles' );
							}

							currentNode = editor.container;
							while ( ( currentNode = currentNode.getParent() ) )
							{
								restoreStyles( currentNode, currentNode.getCustomData( 'maximize_saved_styles' ) );
								currentNode.removeCustomData( 'maximize_saved_styles' );
							}

							// Restore the window scroll position.
							CKEDITOR.env.ie ?
								setTimeout( function() { mainWindow.$.scrollTo( outerScroll.x, outerScroll.y ); }, 0 ) :
								mainWindow.$.scrollTo( outerScroll.x, outerScroll.y );

							// Remove cke_maximized class.
							container.removeClass( 'cke_maximized' );

							if ( shim )
							{
								shim.remove();
								shim = null;
							}

							// Emit a resize event, because this time the size is modified in
							// restoreStyles.
							editor.fire( 'resize' );
						}

						this.toggleState();

						// Toggle button label.
						var button = this.uiItems[ 0 ];
						var label = ( this.state == CKEDITOR.TRISTATE_OFF )
							? lang.maximize : lang.minimize;
						var buttonNode = editor.element.getDocument().getById( button._.id );
						buttonNode.getChild( 1 ).setHtml( label );
						buttonNode.setAttribute( 'title', label );
						buttonNode.setAttribute( 'href', 'javascript:void("' + label + '");' );

						// Restore selection and scroll position in editing area.
						if ( editor.mode == 'wysiwyg' )
						{
							if ( savedSelection )
							{
								// Fixing positioning editor chrome in Firefox break design mode. (#5149)
								CKEDITOR.env.gecko && refreshCursor( editor );

								editor.getSelection().selectRanges(savedSelection);
								var element = editor.getSelection().getStartElement();
								element && element.scrollIntoView( true );
							}

							else
								mainWindow.$.scrollTo( savedScroll.x, savedScroll.y );
						}
						else
						{
							if ( savedSelection )
							{
								$textarea.selectionStart = savedSelection[0];
								$textarea.selectionEnd = savedSelection[1];
							}
							$textarea.scrollLeft = savedScroll[0];
							$textarea.scrollTop = savedScroll[1];
						}

						savedSelection = savedScroll = null;
						savedState = this.state;
					},
					canUndo : false
				} );

			editor.ui.addButton( 'Maximize',
				{
					label : lang.maximize,
					command : 'maximize'
				} );

			// Restore the command state after mode change.
			editor.on( 'mode', function()
				{
					editor.getCommand( 'maximize' ).setState( savedState );
				}, null, null, 100 );
		}
	} );
})();
