﻿/*
Copyright (c) 2003-2010, CKSource - Frederico Knabben. All rights reserved.
For licensing, see LICENSE.html or http://ckeditor.com/license
*/

(function()
{
	CKEDITOR.plugins.add( 'enterkey',
	{
		requires : [ 'keystrokes', 'indent' ],

		init : function( editor )
		{
			var specialKeys = editor.specialKeys;
			specialKeys[ 13 ] = enter;
			specialKeys[ CKEDITOR.SHIFT + 13 ] = shiftEnter;
		}
	});

	CKEDITOR.plugins.enterkey =
	{
		enterBlock : function( editor, mode, range, forceMode )
		{
			// Get the range for the current selection.
			range = range || getRange( editor );

			var doc = range.document;

			// Exit the list when we're inside an empty list item block. (#5376)
			if ( range.checkStartOfBlock() && range.checkEndOfBlock() )
			{
				var path = new CKEDITOR.dom.elementPath( range.startContainer ),
						block = path.block;

				if ( block && ( block.is( 'li' ) || block.getParent().is( 'li' ) ) )
				{
					editor.execCommand( 'outdent' );
					return;
				}
			}

			// Determine the block element to be used.
			var blockTag = ( mode == CKEDITOR.ENTER_DIV ? 'div' : 'p' );

			// Split the range.
			var splitInfo = range.splitBlock( blockTag );

			if ( !splitInfo )
				return;

			// Get the current blocks.
			var previousBlock	= splitInfo.previousBlock,
				nextBlock		= splitInfo.nextBlock;

			var isStartOfBlock	= splitInfo.wasStartOfBlock,
				isEndOfBlock	= splitInfo.wasEndOfBlock;

			var node;

			// If this is a block under a list item, split it as well. (#1647)
			if ( nextBlock )
			{
				node = nextBlock.getParent();
				if ( node.is( 'li' ) )
				{
					nextBlock.breakParent( node );
					nextBlock.move( nextBlock.getNext(), true );
				}
			}
			else if ( previousBlock && ( node = previousBlock.getParent() ) && node.is( 'li' ) )
			{
				previousBlock.breakParent( node );
				range.moveToElementEditStart( previousBlock.getNext() );
				previousBlock.move( previousBlock.getPrevious() );
			}

			// If we have both the previous and next blocks, it means that the
			// boundaries were on separated blocks, or none of them where on the
			// block limits (start/end).
			if ( !isStartOfBlock && !isEndOfBlock )
			{
				// If the next block is an <li> with another list tree as the first
				// child, we'll need to append a filler (<br>/NBSP) or the list item
				// wouldn't be editable. (#1420)
				if ( nextBlock.is( 'li' )
					 && ( node = nextBlock.getFirst( CKEDITOR.dom.walker.invisible( true ) ) )
					 && node.is && node.is( 'ul', 'ol' ) )
					( CKEDITOR.env.ie ? doc.createText( '\xa0' ) : doc.createElement( 'br' ) ).insertBefore( node );

				// Move the selection to the end block.
				if ( nextBlock )
					range.moveToElementEditStart( nextBlock );
			}
			else
			{
				var newBlock;

				/**
				 * Definition list support
				 * @author MindTouch
				 */
				if (  isStartOfBlock && isEndOfBlock && previousBlock.is( 'dt' )  )
				{
					var parent = previousBlock.getParent();
					newBlock = doc.createElement( blockTag );

					previousBlock.remove().moveChildren( newBlock );
					newBlock.insertAfter( parent );

					range.moveToElementEditStart( newBlock );
					range.select();
					return;
				}
				/* END */

				if ( previousBlock )
				{
					/**
					 * Definition list support
					 * @author MindTouch
					 */
					if ( previousBlock.is( 'dt', 'dd' ) )
					{
						newBlock = new CKEDITOR.dom.element( previousBlock.getName() == 'dt' ? 'dd' : 'dt', editor.document );
					}
					// Do not enter this block if it's a header tag, or we are in
					// a Shift+Enter (#77). Create a new block element instead
					// (later in the code).
					else if ( previousBlock.is( 'li' ) || !headerTagRegex.test( previousBlock.getName() ) )
					// if ( previousBlock.is( 'li' ) || !headerTagRegex.test( previousBlock.getName() ) )
					/* END */
					{
						// Otherwise, duplicate the previous block.
						newBlock = previousBlock.clone();
					}
				}
				else if ( nextBlock )
				/**
				 * Definition list support
				 * @author MindTouch
				 */
				{
					if ( nextBlock.getName() == 'dd' )
						newBlock = new CKEDITOR.dom.element( 'dt', editor.document );
					else
						newBlock = nextBlock.clone();
				}
				/* END */

				if ( !newBlock )
					newBlock = doc.createElement( blockTag );
				// Force the enter block unless we're talking of a list item.
				else if ( forceMode && !newBlock.is( 'li' ) )
				{
					newBlock.renameNode( blockTag );
					/**
					 * Don't keep attributes
					 * 
					 * @see #0008224 (3)
					 * @author MindTouch
					 */
					var attributes = newBlock.$.attributes;
					for ( var i = 0 ; i < attributes.length ; i++ )
					{
						var attr = attributes.item( i );
						newBlock.removeAttribute( attr.nodeName );
					}
					/* END */
				}

				// Recreate the inline elements tree, which was available
				// before hitting enter, so the same styles will be available in
				// the new block.
				var elementPath = splitInfo.elementPath;
				if ( elementPath )
				{
					for ( var i = 0, len = elementPath.elements.length ; i < len ; i++ )
					{
						var element = elementPath.elements[ i ];

						if ( element.equals( elementPath.block ) || element.equals( elementPath.blockLimit ) )
							break;

						if ( CKEDITOR.dtd.$removeEmpty[ element.getName() ] )
						{
							element = element.clone();
							newBlock.moveChildren( element );
							newBlock.append( element );
						}
					}
				}

				if ( !CKEDITOR.env.ie )
					newBlock.appendBogus();

				range.insertNode( newBlock );

				// This is tricky, but to make the new block visible correctly
				// we must select it.
				// The previousBlock check has been included because it may be
				// empty if we have fixed a block-less space (like ENTER into an
				// empty table cell).
				if ( CKEDITOR.env.ie && isStartOfBlock && ( !isEndOfBlock || !previousBlock.getChildCount() ) )
				{
					// Move the selection to the new block.
					range.moveToElementEditStart( isEndOfBlock ? previousBlock : newBlock );
					range.select();
				}

				// Move the selection to the new block.
				range.moveToElementEditStart( isStartOfBlock && !isEndOfBlock ? nextBlock : newBlock );
		}

			if ( !CKEDITOR.env.ie )
			{
				if ( nextBlock )
				{
					// If we have split the block, adds a temporary span at the
					// range position and scroll relatively to it.
					var tmpNode = doc.createElement( 'span' );

					// We need some content for Safari.
					tmpNode.setHtml( '&nbsp;' );

					range.insertNode( tmpNode );
					tmpNode.scrollIntoView();
					range.deleteContents();
				}
				else
				{
					// We may use the above scroll logic for the new block case
					// too, but it gives some weird result with Opera.
					newBlock.scrollIntoView();
				}
			}

			range.select();
		},

		enterBr : function( editor, mode, range, forceMode )
		{
			// Get the range for the current selection.
			range = range || getRange( editor );

			var doc = range.document;

			// Determine the block element to be used.
			var blockTag = ( mode == CKEDITOR.ENTER_DIV ? 'div' : 'p' );

			var isEndOfBlock = range.checkEndOfBlock();

			var elementPath = new CKEDITOR.dom.elementPath( editor.getSelection().getStartElement() );

			var startBlock = elementPath.block,
				startBlockTag = startBlock && elementPath.block.getName();

			var isPre = false;

			if ( !forceMode && startBlockTag == 'li' )
			{
				enterBlock( editor, mode, range, forceMode );
				return;
			}

			// If we are at the end of a header block.
			if ( !forceMode && isEndOfBlock && headerTagRegex.test( startBlockTag ) )
			{
				// Insert a <br> after the current paragraph.
				doc.createElement( 'br' ).insertAfter( startBlock );

				// A text node is required by Gecko only to make the cursor blink.
				if ( CKEDITOR.env.gecko )
					doc.createText( '' ).insertAfter( startBlock );

				// IE has different behaviors regarding position.
				range.setStartAt( startBlock.getNext(), CKEDITOR.env.ie ? CKEDITOR.POSITION_BEFORE_START : CKEDITOR.POSITION_AFTER_START );
			}
			else
			{
				var lineBreak;

				isPre = ( startBlockTag == 'pre' );

				// Gecko prefers <br> as line-break inside <pre> (#4711).
				if ( isPre && !CKEDITOR.env.gecko )
					lineBreak = doc.createText( CKEDITOR.env.ie ? '\r' : '\n' );
				else
					lineBreak = doc.createElement( 'br' );

				range.deleteContents();
				range.insertNode( lineBreak );

				// A text node is required by Gecko only to make the cursor blink.
				// We need some text inside of it, so the bogus <br> is properly
				// created.
				if ( !CKEDITOR.env.ie )
					doc.createText( '\ufeff' ).insertAfter( lineBreak );

				// If we are at the end of a block, we must be sure the bogus node is available in that block.
				if ( isEndOfBlock && !CKEDITOR.env.ie )
					lineBreak.getParent().appendBogus();

				// Now we can remove the text node contents, so the caret doesn't
				// stop on it.
				if ( !CKEDITOR.env.ie )
					lineBreak.getNext().$.nodeValue = '';
				// IE has different behavior regarding position.
				if ( CKEDITOR.env.ie )
					range.setStartAt( lineBreak, CKEDITOR.POSITION_AFTER_END );
				else
					range.setStartAt( lineBreak.getNext(), CKEDITOR.POSITION_AFTER_START );

				/**
				 * Keep indentation for pre blocks
				 * 
				 * @see #0008041
				 * @author MindTouch
				 * 
				 */
//				if ( isPre )
//				{
//					var prev = lineBreak.getPrevious();
//
//					while ( prev && prev.type == CKEDITOR.NODE_TEXT )
//					{
//						var prevLine = prev.getText();
//						var lines = prevLine.split( /[\r\n]+/ );
//
//						// IE puts line breaks to the start of the line
//						// and splits line started with \r to array with one element
//						var isIELineBreak = ( CKEDITOR.env.ie && /^\r/.test( prevLine ) );
//
//						if ( lines.length == 1 && !isIELineBreak )
//						{
//							var prevNode = prev.getPrevious();
//
//							if ( prevNode && prevNode.type == CKEDITOR.NODE_TEXT && !/^[\r\n]+$/.test( prevNode.getText() ) )
//							{
//								prev = prevNode;
//								continue;
//							}
//						}
//						else
//						{
//							prevLine = lines[ lines.length - 1 ];
//						}
//
//						var re = /^((?:\s|&nbsp;|\u00A0|&#160;)+)/;
//						var matches = re.exec( prevLine );
//
//						if ( matches )
//						{
//							var node = new CKEDITOR.dom.text( matches[1], editor.document );
//							range.insertNode( node );
//							range.setStartAt( node, CKEDITOR.POSITION_BEFORE_END );
//							break;
//						}
//
//						if ( lines.length == 1 && !isIELineBreak )
//						{
//							prev = prev && prev.getPrevious();
//						}
//						else
//						{
//							break;
//						}
//					}
//				}
				/* END */

				// Scroll into view, for non IE.
				if ( !CKEDITOR.env.ie )
				{
					var dummy = null;

					// BR is not positioned in Opera and Webkit.
					if ( !CKEDITOR.env.gecko )
					{
						dummy = doc.createElement( 'span' );
						// We need have some contents for Webkit to position it
						// under parent node. ( #3681)
						dummy.setHtml('&nbsp;');
					}
					else
						dummy = doc.createElement( 'br' );

					dummy.insertBefore( lineBreak.getNext() );
					dummy.scrollIntoView();
					dummy.remove();
				}
			}

			// This collapse guarantees the cursor will be blinking.
			range.collapse( true );

			range.select( isPre );
		}
	};

	var plugin = CKEDITOR.plugins.enterkey,
		enterBr = plugin.enterBr,
		enterBlock = plugin.enterBlock,
		headerTagRegex = /^h[1-6]$/;

	function shiftEnter( editor )
	{
		// Only effective within document.
		if ( editor.mode != 'wysiwyg' )
			return false;

		// On SHIFT+ENTER:
		// 1. We want to enforce the mode to be respected, instead
		// of cloning the current block. (#77)
		// 2. Always perform a block break when inside <pre> (#5402).
		if ( editor.getSelection().getStartElement().hasAscendant( 'pre', true ) )
		{
			setTimeout( function() { enterBlock( editor, editor.config.enterMode, null, true ); }, 0 );
			return true;
		}
		else
			return enter( editor, editor.config.shiftEnterMode, true );
	}

	function enter( editor, mode, forceMode )
	{
		forceMode = editor.config.forceEnterMode || forceMode;

		// Only effective within document.
		if ( editor.mode != 'wysiwyg' )
			return false;

		if ( !mode )
			mode = editor.config.enterMode;

		// Use setTimout so the keys get cancelled immediatelly.
		setTimeout( function()
			{
				editor.fire( 'saveSnapshot' );	// Save undo step.
				if ( mode == CKEDITOR.ENTER_BR || editor.getSelection().getStartElement().hasAscendant( 'pre', true ) )
					enterBr( editor, mode, null, forceMode );
				else
					enterBlock( editor, mode, null, forceMode );

			}, 0 );

		return true;
	}


	function getRange( editor )
	{
		// Get the selection ranges.
		var ranges = editor.getSelection().getRanges();

		// Delete the contents of all ranges except the first one.
		for ( var i = ranges.length - 1 ; i > 0 ; i-- )
		{
			ranges[ i ].deleteContents();
		}

		// Return the first range.
		return ranges[ 0 ];
	}
})();
