﻿/*
Copyright (c) 2003-2010, CKSource - Frederico Knabben. All rights reserved.
For licensing, see LICENSE.html or http://ckeditor.com/license
*/

CKEDITOR.plugins.add( 'styles',
{
	requires : [ 'selection' ]
});

/**
 * Registers a function to be called whenever a style changes its state in the
 * editing area. The current state is passed to the function. The possible
 * states are {@link CKEDITOR.TRISTATE_ON} and {@link CKEDITOR.TRISTATE_OFF}.
 * @param {CKEDITOR.style} The style to be watched.
 * @param {Function} The function to be called when the style state changes.
 * @example
 * // Create a style object for the &lt;b&gt; element.
 * var style = new CKEDITOR.style( { element : 'b' } );
 * var editor = CKEDITOR.instances.editor1;
 * editor.attachStyleStateChange( style, function( state )
 *     {
 *         if ( state == CKEDITOR.TRISTATE_ON )
 *             alert( 'The current state for the B element is ON' );
 *         else
 *             alert( 'The current state for the B element is OFF' );
 *     });
 */
CKEDITOR.editor.prototype.attachStyleStateChange = function( style, callback )
{
	// Try to get the list of attached callbacks.
	var styleStateChangeCallbacks = this._.styleStateChangeCallbacks;

	// If it doesn't exist, it means this is the first call. So, let's create
	// all the structure to manage the style checks and the callback calls.
	if ( !styleStateChangeCallbacks )
	{
		// Create the callbacks array.
		styleStateChangeCallbacks = this._.styleStateChangeCallbacks = [];

		// Attach to the selectionChange event, so we can check the styles at
		// that point.
		this.on( 'selectionChange', function( ev )
			{
				// Loop throw all registered callbacks.
				for ( var i = 0 ; i < styleStateChangeCallbacks.length ; i++ )
				{
					var callback = styleStateChangeCallbacks[ i ];

					// Check the current state for the style defined for that
					// callback.
					var currentState = callback.style.checkActive( ev.data.path ) ? CKEDITOR.TRISTATE_ON : CKEDITOR.TRISTATE_OFF;

					// If the state changed since the last check.
					if ( callback.state !== currentState )
					{
						// Call the callback function, passing the current
						// state to it.
						callback.fn.call( this, currentState );

						// Save the current state, so it can be compared next
						// time.
						callback.state !== currentState;
					}
				}
			});
	}

	// Save the callback info, so it can be checked on the next occurence of
	// selectionChange.
	styleStateChangeCallbacks.push( { style : style, fn : callback } );
};

CKEDITOR.STYLE_BLOCK = 1;
CKEDITOR.STYLE_INLINE = 2;
CKEDITOR.STYLE_OBJECT = 3;

(function()
{
	/**
	 * Added blockquote to block elements list
	 *
	 * @author MindTouch
	 */
//	var blockElements	= { address:1,div:1,h1:1,h2:1,h3:1,h4:1,h5:1,h6:1,p:1,pre:1 };
	var blockElements	= { address:1,div:1,h1:1,h2:1,h3:1,h4:1,h5:1,h6:1,p:1,pre:1,blockquote:1 };
	/* END */
	var objectElements	= { a:1,embed:1,hr:1,img:1,li:1,object:1,ol:1,table:1,td:1,tr:1,th:1,ul:1,dl:1,dt:1,dd:1,form:1};

	var semicolonFixRegex = /\s*(?:;\s*|$)/;

	CKEDITOR.style = function( styleDefinition, variablesValues )
	{
		if ( variablesValues )
		{
			styleDefinition = CKEDITOR.tools.clone( styleDefinition );

			replaceVariables( styleDefinition.attributes, variablesValues );
			replaceVariables( styleDefinition.styles, variablesValues );
		}

		var element = this.element = ( styleDefinition.element || '*' ).toLowerCase();

		this.type =
			( element == '#' || blockElements[ element ] ) ?
				CKEDITOR.STYLE_BLOCK
			: objectElements[ element ] ?
				CKEDITOR.STYLE_OBJECT
			:
				CKEDITOR.STYLE_INLINE;

		this._ =
		{
			definition : styleDefinition
		};
	};

	CKEDITOR.style.prototype =
	{
		apply : function( document )
		{
			applyStyle.call( this, document, false );
		},

		remove : function( document )
		{
			applyStyle.call( this, document, true );
		},

		applyToRange : function( range )
		{
			return ( this.applyToRange =
						this.type == CKEDITOR.STYLE_INLINE ?
							applyInlineStyle
						: this.type == CKEDITOR.STYLE_BLOCK ?
							applyBlockStyle
						: this.type == CKEDITOR.STYLE_OBJECT ?
							applyObjectStyle
						: null ).call( this, range );
		},

		removeFromRange : function( range )
		{
			/**
			 * Allow to remove block style
			 *
			 * @see #0007733
			 * @author MindTouch
			 *
			 */
			return ( this.removeFromRange =
						this.type == CKEDITOR.STYLE_INLINE ?
							removeInlineStyle
						: this.type == CKEDITOR.STYLE_BLOCK ?
							removeBlockStyle
						: this.type == CKEDITOR.STYLE_OBJECT ?
							removeObjectStyle
						: null ).call( this, range );
			/* END */
		},

		applyToObject : function( element )
		{
			setupElement( element, this );
		},

		/**
		 * Get the style state inside an element path. Returns "true" if the
		 * element is active in the path.
		 */
		checkActive : function( elementPath )
		{
			switch ( this.type )
			{
				case CKEDITOR.STYLE_BLOCK :
					return this.checkElementRemovable( elementPath.block || elementPath.blockLimit, true );

				case CKEDITOR.STYLE_OBJECT :
				case CKEDITOR.STYLE_INLINE :

					var elements = elementPath.elements;

					for ( var i = 0, element ; i < elements.length ; i++ )
					{
						element = elements[ i ];

						if ( this.type == CKEDITOR.STYLE_INLINE
							  && ( element == elementPath.block || element == elementPath.blockLimit ) )
							continue;

						if( this.type == CKEDITOR.STYLE_OBJECT
							 && !( element.getName() in objectElements ) )
								continue;

						if ( this.checkElementRemovable( element, true ) )
							return true;
					}
			}
			return false;
		},

		checkApplicable : function( elementPath )
		{
			switch ( this.type )
			{
				case CKEDITOR.STYLE_INLINE :
				case CKEDITOR.STYLE_BLOCK :
					break;

				case CKEDITOR.STYLE_OBJECT :
					return elementPath.lastElement.getAscendant( this.element, true );
			}

			return true;
		},

		// Checks if an element, or any of its attributes, is removable by the
		// current style definition.
		checkElementRemovable : function( element, fullMatch )
		{
			if ( !element )
				return false;

			var def = this._.definition,
				attribs;

			// If the element name is the same as the style name.
			if ( element.getName() == this.element )
			{
				// If no attributes are defined in the element.
				if ( !fullMatch && !element.hasAttributes() )
					return true;

				attribs = getAttributesForComparison( def );

				if ( attribs._length )
				{
					for ( var attName in attribs )
					{
						/**
						 * Style attribute protection
						 * Also skip _cke_saved_style attribute
						 * @author MindTouch
						 */
						if ( attName == '_length' || attName == '_cke_saved_style' )
						// if ( attName == '_length' )
						/* END */
							continue;

						var elementAttr = element.getAttribute( attName ) || '';
						if ( attName == 'style' ?
							compareCssText( attribs[ attName ], normalizeCssText( elementAttr, false ) )
							: attribs[ attName ] == elementAttr  )
						{
							if ( !fullMatch )
								return true;
						}
						else if ( fullMatch )
								return false;
					}
					if ( fullMatch )
						return true;
				}
				else
					return true;
			}

			// Check if the element can be somehow overriden.
			var override = getOverrides( this )[ element.getName() ] ;
			if ( override )
			{
				// If no attributes have been defined, remove the element.
				if ( !( attribs = override.attributes ) )
					return true;

				for ( var i = 0 ; i < attribs.length ; i++ )
				{
					attName = attribs[i][0];
					var actualAttrValue = element.getAttribute( attName );
					if ( actualAttrValue )
					{
						var attValue = attribs[i][1];

						// Remove the attribute if:
						//    - The override definition value is null;
						//    - The override definition value is a string that
						//      matches the attribute value exactly.
						//    - The override definition value is a regex that
						//      has matches in the attribute value.
						if ( attValue === null ||
								( typeof attValue == 'string' && actualAttrValue == attValue ) ||
								attValue.test( actualAttrValue ) )
							return true;
					}
				}
			}
			return false;
		},

		// Builds the preview HTML based on the styles definition.
		buildPreview : function()
		{
			var styleDefinition = this._.definition,
				html = [],
				elementName = styleDefinition.element;

			// Avoid <bdo> in the preview.
			if ( elementName == 'bdo' )
				elementName = 'span';

			html = [ '<', elementName ];

			// Assign all defined attributes.
			var attribs	= styleDefinition.attributes;
			if ( attribs )
			{
				for ( var att in attribs )
				{
					html.push( ' ', att, '="', attribs[ att ], '"' );
				}
			}

			// Assign the style attribute.
			var cssStyle = CKEDITOR.style.getStyleText( styleDefinition );
			if ( cssStyle )
				html.push( ' style="', cssStyle, '"' );

			html.push( '>', styleDefinition.name, '</', elementName, '>' );

			return html.join( '' );
		}
	};

	// Build the cssText based on the styles definition.
	CKEDITOR.style.getStyleText = function( styleDefinition )
	{
		// If we have already computed it, just return it.
		var stylesDef = styleDefinition._ST;
		if ( stylesDef )
			return stylesDef;

		stylesDef = styleDefinition.styles;

		// Builds the StyleText.
		var stylesText = ( styleDefinition.attributes && styleDefinition.attributes[ 'style' ] ) || '',
				specialStylesText = '';

		if ( stylesText.length )
			stylesText = stylesText.replace( semicolonFixRegex, ';' );

		for ( var style in stylesDef )
		{
			var styleVal = stylesDef[ style ],
					text = ( style + ':' + styleVal ).replace( semicolonFixRegex, ';' );

			// Some browsers don't support 'inherit' property value, leave them intact. (#5242)
			if ( styleVal == 'inherit' )
				specialStylesText += text;
			else
				stylesText += text;
		}

		// Browsers make some changes to the style when applying them. So, here
		// we normalize it to the browser format.
		if ( stylesText.length )
			stylesText = normalizeCssText( stylesText );

		stylesText += specialStylesText;

		// Return it, saving it to the next request.
		return ( styleDefinition._ST = stylesText );
	};

	function applyInlineStyle( range )
	{
		var document = range.document;

		if ( range.collapsed )
		{
			// Create the element to be inserted in the DOM.
			var collapsedElement = getElement( this, document );

			// Insert the empty element into the DOM at the range position.
			range.insertNode( collapsedElement );

			// Place the selection right inside the empty element.
			range.moveToPosition( collapsedElement, CKEDITOR.POSITION_BEFORE_END );

			return;
		}

		var elementName = this.element;
		/**
		 * Clone the definition
		 *
		 * @see 0008222
		 * @author MindTouch
		 */
//		var def = this._.definition;
		var def = CKEDITOR.tools.clone( this._.definition );
		/* END */
		var isUnknownElement;

		// Get the DTD definition for the element. Defaults to "span".
		var dtd = CKEDITOR.dtd[ elementName ] || ( isUnknownElement = true, CKEDITOR.dtd.span );

		// Bookmark the range so we can re-select it after processing.
		var bookmark = range.createBookmark();

		// Expand the range.
		range.enlarge( CKEDITOR.ENLARGE_ELEMENT );
		range.trim();

		// Get the first node to be processed and the last, which concludes the
		// processing.
		var boundaryNodes = range.createBookmark(),
			firstNode = boundaryNodes.startNode,
			lastNode = boundaryNodes.endNode;

		var currentNode = firstNode;

		var styleRange;

		while ( currentNode )
		{
			var applyStyle = false;

			if ( currentNode.equals( lastNode ) )
			{
				currentNode = null;
				applyStyle = true;
			}
			else
			{
				var nodeType = currentNode.type;
				var nodeName = nodeType == CKEDITOR.NODE_ELEMENT ? currentNode.getName() : null;

				if ( nodeName && currentNode.getAttribute( '_fck_bookmark' ) )
				{
					currentNode = currentNode.getNextSourceNode( true );
					continue;
				}

				// Check if the current node can be a child of the style element.
				if ( !nodeName || ( dtd[ nodeName ]
					&& ( currentNode.getPosition( lastNode ) | CKEDITOR.POSITION_PRECEDING | CKEDITOR.POSITION_IDENTICAL | CKEDITOR.POSITION_IS_CONTAINED ) == ( CKEDITOR.POSITION_PRECEDING + CKEDITOR.POSITION_IDENTICAL + CKEDITOR.POSITION_IS_CONTAINED )
					&& ( !def.childRule || def.childRule( currentNode ) ) ) )
				{
					var currentParent = currentNode.getParent();

					// Check if the style element can be a child of the current
					// node parent or if the element is not defined in the DTD.
					if ( currentParent
						&& ( ( currentParent.getDtd() || CKEDITOR.dtd.span )[ elementName ] || isUnknownElement )
						&& ( !def.parentRule || def.parentRule( currentParent ) ) )
					{
						// This node will be part of our range, so if it has not
						// been started, place its start right before the node.
						// In the case of an element node, it will be included
						// only if it is entirely inside the range.
						if ( !styleRange && ( !nodeName || !CKEDITOR.dtd.$removeEmpty[ nodeName ] || ( currentNode.getPosition( lastNode ) | CKEDITOR.POSITION_PRECEDING | CKEDITOR.POSITION_IDENTICAL | CKEDITOR.POSITION_IS_CONTAINED ) == ( CKEDITOR.POSITION_PRECEDING + CKEDITOR.POSITION_IDENTICAL + CKEDITOR.POSITION_IS_CONTAINED ) ) )
						{
							styleRange = new CKEDITOR.dom.range( document );
							styleRange.setStartBefore( currentNode );
						}

						// Non element nodes, or empty elements can be added
						// completely to the range.
						if ( nodeType == CKEDITOR.NODE_TEXT || ( nodeType == CKEDITOR.NODE_ELEMENT && !currentNode.getChildCount() ) )
						{
							var includedNode = currentNode;
							var parentNode;

							// This node is about to be included completelly, but,
							// if this is the last node in its parent, we must also
							// check if the parent itself can be added completelly
							// to the range.
							while ( !includedNode.$.nextSibling
								&& ( parentNode = includedNode.getParent(), dtd[ parentNode.getName() ] )
								&& ( parentNode.getPosition( firstNode ) | CKEDITOR.POSITION_FOLLOWING | CKEDITOR.POSITION_IDENTICAL | CKEDITOR.POSITION_IS_CONTAINED ) == ( CKEDITOR.POSITION_FOLLOWING + CKEDITOR.POSITION_IDENTICAL + CKEDITOR.POSITION_IS_CONTAINED )
								&& ( !def.childRule || def.childRule( parentNode ) ) )
							{
								includedNode = parentNode;
							}

							styleRange.setEndAfter( includedNode );

							// If the included node still is the last node in its
							// parent, it means that the parent can't be included
							// in this style DTD, so apply the style immediately.
							if ( !includedNode.$.nextSibling )
								applyStyle = true;

						}
					}
					else
						applyStyle = true;
				}
				else
					applyStyle = true;

				// Get the next node to be processed.
				currentNode = currentNode.getNextSourceNode();
			}

			// Apply the style if we have something to which apply it.
			if ( applyStyle && styleRange && !styleRange.collapsed )
			{
				// Build the style element, based on the style object definition.
				var styleNode = getElement( this, document );

				// Get the element that holds the entire range.
				var parent = styleRange.getCommonAncestor();

				// Loop through the parents, removing the redundant attributes
				// from the element to be applied.
				while ( styleNode && parent )
				{
					if ( parent.getName() == elementName )
					{
						for ( var attName in def.attributes )
						{
							if ( styleNode.getAttribute( attName ) == parent.getAttribute( attName ) )
								styleNode.removeAttribute( attName );
						}

						for ( var styleName in def.styles )
						{
							if ( styleNode.getStyle( styleName ) == parent.getStyle( styleName ) )
								styleNode.removeStyle( styleName );
							/**
							 * Don't remove style from parent elements
							 * if the current parent already has style
							 *
							 * @see 0008222
							 * @author MindTouch
							 */
							else if ( parent.getStyle( styleName ).length > 0 )
								delete def.styles[ styleName ];
							/* END */
						}

						if ( !styleNode.hasAttributes() )
						{
							styleNode = null;
							break;
						}
					}

					parent = parent.getParent();
				}

				if ( styleNode )
				{
					// Move the contents of the range to the style element.
					styleRange.extractContents().appendTo( styleNode );

					// Here we do some cleanup, removing all duplicated
					// elements from the style element.
					removeFromInsideElement( this, styleNode );

					// Insert it into the range position (it is collapsed after
					// extractContents.
					styleRange.insertNode( styleNode );

					// Let's merge our new style with its neighbors, if possible.
					styleNode.mergeSiblings();

					// As the style system breaks text nodes constantly, let's normalize
					// things for performance.
					// With IE, some paragraphs get broken when calling normalize()
					// repeatedly. Also, for IE, we must normalize body, not documentElement.
					// IE is also known for having a "crash effect" with normalize().
					// We should try to normalize with IE too in some way, somewhere.
					if ( !CKEDITOR.env.ie )
						styleNode.$.normalize();
				}

				// Style applied, let's release the range, so it gets
				// re-initialization in the next loop.
				styleRange = null;
			}
		}

		firstNode.remove();
		lastNode.remove();
		range.moveToBookmark( bookmark );
		// Minimize the result range to exclude empty text nodes. (#5374)
		range.shrink( CKEDITOR.SHRINK_TEXT );
	}

	function removeInlineStyle( range )
	{
		/**
		 * Create bookmark before enrarging of the range
		 *
		 * @see 0007977: Problem during applying of text color to table cell
		 * @author MindTouch
		 */
		var bookmark = range.createBookmark(),
			startNode = bookmark.startNode;
		/* END */

		/*
		 * Make sure our range has included all "collpased" parent inline nodes so
		 * that our operation logic can be simpler.
		 */
		range.enlarge( CKEDITOR.ENLARGE_ELEMENT );

		/**
		 * @author MindTouch
		 */
//		var bookmark = range.createBookmark(),
//			startNode = bookmark.startNode;
		/* END */

		if ( range.collapsed )
		{

			var startPath = new CKEDITOR.dom.elementPath( startNode.getParent() ),
				// The topmost element in elementspatch which we should jump out of.
				boundaryElement;


			for ( var i = 0, element ; i < startPath.elements.length
					&& ( element = startPath.elements[i] ) ; i++ )
			{
				/*
				 * 1. If it's collaped inside text nodes, try to remove the style from the whole element.
				 *
				 * 2. Otherwise if it's collapsed on element boundaries, moving the selection
				 *  outside the styles instead of removing the whole tag,
				 *  also make sure other inner styles were well preserverd.(#3309)
				 */
				if ( element == startPath.block || element == startPath.blockLimit )
					break;

				if ( this.checkElementRemovable( element ) )
				{
					var endOfElement = range.checkBoundaryOfElement( element, CKEDITOR.END ),
							startOfElement = !endOfElement && range.checkBoundaryOfElement( element, CKEDITOR.START );
					if ( startOfElement || endOfElement )
					{
						boundaryElement = element;
						boundaryElement.match = startOfElement ? 'start' : 'end';
					}
					else
					{
						/*
						 * Before removing the style node, there may be a sibling to the style node
						 * that's exactly the same to the one to be removed. To the user, it makes
						 * no difference that they're separate entities in the DOM tree. So, merge
						 * them before removal.
						 */
						element.mergeSiblings();
						removeFromElement( this, element );

					}
				}
			}

			// Re-create the style tree after/before the boundary element,
			// the replication start from bookmark start node to define the
			// new range.
			if ( boundaryElement )
			{
				var clonedElement = startNode;
				for ( i = 0 ;; i++ )
				{
					var newElement = startPath.elements[ i ];
					if ( newElement.equals( boundaryElement ) )
						break;
					// Avoid copying any matched element.
					else if ( newElement.match )
						continue;
					else
						newElement = newElement.clone();
					newElement.append( clonedElement );
					clonedElement = newElement;
				}
				clonedElement[ boundaryElement.match == 'start' ?
							'insertBefore' : 'insertAfter' ]( boundaryElement );
			}
		}
		else
		{
			/*
			 * Now our range isn't collapsed. Lets walk from the start node to the end
			 * node via DFS and remove the styles one-by-one.
			 */
			var endNode = bookmark.endNode,
				me = this;

			/*
			 * Find out the style ancestor that needs to be broken down at startNode
			 * and endNode.
			 */
			function breakNodes()
			{
				var startPath = new CKEDITOR.dom.elementPath( startNode.getParent() ),
					endPath = new CKEDITOR.dom.elementPath( endNode.getParent() ),
					breakStart = null,
					breakEnd = null;
				for ( var i = 0 ; i < startPath.elements.length ; i++ )
				{
					var element = startPath.elements[ i ];

					if ( element == startPath.block || element == startPath.blockLimit )
						break;

					if ( me.checkElementRemovable( element ) )
						breakStart = element;
				}
				for ( i = 0 ; i < endPath.elements.length ; i++ )
				{
					element = endPath.elements[ i ];

					if ( element == endPath.block || element == endPath.blockLimit )
						break;

					if ( me.checkElementRemovable( element ) )
						breakEnd = element;
				}

				if ( breakEnd )
					endNode.breakParent( breakEnd );
				if ( breakStart )
					startNode.breakParent( breakStart );
			}
			breakNodes();

			// Now, do the DFS walk.
			var currentNode = startNode.getNext();
			while ( !currentNode.equals( endNode ) )
			{
				/*
				 * Need to get the next node first because removeFromElement() can remove
				 * the current node from DOM tree.
				 */
				var nextNode = currentNode.getNextSourceNode();
				if ( currentNode.type == CKEDITOR.NODE_ELEMENT && this.checkElementRemovable( currentNode ) )
				{
					// Remove style from element or overriding element.
					if ( currentNode.getName() == this.element )
						removeFromElement( this, currentNode );
					else
						removeOverrides( currentNode, getOverrides( this )[ currentNode.getName() ] );

					/*
					 * removeFromElement() may have merged the next node with something before
					 * the startNode via mergeSiblings(). In that case, the nextNode would
					 * contain startNode and we'll have to call breakNodes() again and also
					 * reassign the nextNode to something after startNode.
					 */
					if ( nextNode.type == CKEDITOR.NODE_ELEMENT && nextNode.contains( startNode ) )
					{
						breakNodes();
						nextNode = startNode.getNext();
					}
				}
				currentNode = nextNode;
			}
		}

		range.moveToBookmark( bookmark );
}

	function applyObjectStyle( range )
	{
		var root = range.getCommonAncestor( true, true ),
				element = root.getAscendant( this.element, true );
		element && setupElement( element, this );
	}

	function applyBlockStyle( range )
	{
		// Serializible bookmarks is needed here since
		// elements may be merged.
		var bookmark = range.createBookmark( true );

		var iterator = range.createIterator();
		iterator.enforceRealBlocks = true;

		// make recognize <br /> tag as a separator in ENTER_BR mode (#5121)
		if ( this._.enterMode )
			iterator.enlargeBr = ( this._.enterMode != CKEDITOR.ENTER_BR );

		var block;
		var doc = range.document;
		var previousPreBlock;

		while ( ( block = iterator.getNextParagraph() ) )		// Only one =
		{
			var newBlock = getElement( this, doc );
			replaceBlock( block, newBlock );
		}

		range.moveToBookmark( bookmark );
	}

	/**
	 * Remove block style
	 *
	 * @see #0007733
	 * @author MindTouch
	 *
	 */
	function removeBlockStyle( range )
	{
		// Serializible bookmarks is needed here since
		// elements may be merged.
		var bookmark = range.createBookmark( true );

		var iterator = range.createIterator();
		iterator.enforceRealBlocks = true;

		var block;
		while ( ( block = iterator.getNextParagraph() ) )		// Only one =
		{
			if ( this.checkElementRemovable( block ) )
			{
				removeFromElement( this, block );
			}
		}

		range.moveToBookmark( bookmark );
	}

	function removeObjectStyle( range )
	{
		var root = range.getCommonAncestor( true, true ),
				element = root.getAscendant( this.element, true );

		if ( element && this.checkElementRemovable( element ) )
		{
			removeFromElement( this, element );
		}
	}
	/* END */

	// Replace the original block with new one, with special treatment
	// for <pre> blocks to make sure content format is well preserved, and merging/splitting adjacent
	// when necessary.(#3188)
	function replaceBlock( block, newBlock )
	{
		var newBlockIsPre	= newBlock.is( 'pre' );
		var blockIsPre		= block.is( 'pre' );

		var isToPre	= newBlockIsPre && !blockIsPre;
		var isFromPre	= !newBlockIsPre && blockIsPre;

		if ( isToPre )
			newBlock = toPre( block, newBlock );
		else if ( isFromPre )
			// Split big <pre> into pieces before start to convert.
			newBlock = fromPres( splitIntoPres( block ), newBlock );
		else
			block.moveChildren( newBlock );

		newBlock.replace( block );

		if ( newBlockIsPre )
		{
			// Merge previous <pre> blocks.
			/**
			 * Don't merge pre blocks
			 *
			 * @see #0007743
			 * @author MindTouch
			 */
			// mergePre( newBlock );
			/* END */
		}
	}

	/**
	 * Merge a <pre> block with a previous sibling if available.
	 */
	function mergePre( preBlock )
	{
		var previousBlock;
		if ( !( ( previousBlock = preBlock.getPreviousSourceNode( true, CKEDITOR.NODE_ELEMENT ) )
				 && previousBlock.is
				 && previousBlock.is( 'pre') ) )
			return;

		// Merge the previous <pre> block contents into the current <pre>
		// block.
		//
		// Another thing to be careful here is that currentBlock might contain
		// a '\n' at the beginning, and previousBlock might contain a '\n'
		// towards the end. These new lines are not normally displayed but they
		// become visible after merging.
		var mergedHtml = replace( previousBlock.getHtml(), /\n$/, '' ) + '\n\n' +
				replace( preBlock.getHtml(), /^\n/, '' ) ;

		// Krugle: IE normalizes innerHTML from <pre>, breaking whitespaces.
		if ( CKEDITOR.env.ie )
		{
			/**
			 * Copy pre attributes
			 * @author MindTouch
			 */
			// preBlock.$.outerHTML = '<pre>' + mergedHtml + '</pre>';
			var temp = preBlock.getDocument().createElement( 'div' ),
				preBlockId = preBlock.getAttribute( 'id' );
				tempId = 'cke_temp_' + CKEDITOR.tools.getNextNumber();
			preBlock.copyAttributes( temp );
			preBlock.$.outerHTML = '<pre id="' + tempId + '">' + mergedHtml + '</pre>';
			preBlock = preBlock.getDocument().getById( tempId );
			temp.copyAttributes( preBlock );
			if ( preBlockId )
				preBlock.setAttribute( 'id', preBlockId );
			else
				preBlock.removeAttribute( 'id' );
			/* END */
		}
		else
			preBlock.setHtml( mergedHtml );

		previousBlock.remove();
	}

	/**
	 * Split into multiple <pre> blocks separated by double line-break.
	 * @param preBlock
	 */
	function splitIntoPres( preBlock )
	{
		// Exclude the ones at header OR at tail,
		// and ignore bookmark content between them.
		var duoBrRegex = /(\S\s*)\n(?:\s|(<span[^>]+_fck_bookmark.*?\/span>))*\n(?!$)/gi,
			blockName = preBlock.getName(),
			splitedHtml = replace( preBlock.getOuterHtml(),
				duoBrRegex,
				function( match, charBefore, bookmark )
				{
				  return charBefore + '</pre>' + bookmark + '<pre>';
				} );

		var pres = [];
		splitedHtml.replace( /<pre\b.*?>([\s\S]*?)<\/pre>/gi, function( match, preContent ){
			pres.push( preContent );
		} );
		return pres;
	}

	// Wrapper function of String::replace without considering of head/tail bookmarks nodes.
	function replace( str, regexp, replacement )
	{
		var headBookmark = '',
			tailBookmark = '';

		str = str.replace( /(^<span[^>]+_fck_bookmark.*?\/span>)|(<span[^>]+_fck_bookmark.*?\/span>$)/gi,
			function( str, m1, m2 ){
					m1 && ( headBookmark = m1 );
					m2 && ( tailBookmark = m2 );
				return '';
			} );
		return headBookmark + str.replace( regexp, replacement ) + tailBookmark;
	}
	/**
	 * Converting a list of <pre> into blocks with format well preserved.
	 */
	function fromPres( preHtmls, newBlock )
	{
		var docFrag = new CKEDITOR.dom.documentFragment( newBlock.getDocument() );
		for ( var i = 0 ; i < preHtmls.length ; i++ )
		{
			var blockHtml = preHtmls[ i ];

			// 1. Trim the first and last line-breaks immediately after and before <pre>,
			// they're not visible.
			 blockHtml =  blockHtml.replace( /(\r\n|\r)/g, '\n' ) ;
			 blockHtml = replace(  blockHtml, /^[ \t]*\n/, '' ) ;
			 blockHtml = replace(  blockHtml, /\n$/, '' ) ;
			// 2. Convert spaces or tabs at the beginning or at the end to &nbsp;
			 blockHtml = replace(  blockHtml, /^[ \t]+|[ \t]+$/g, function( match, offset, s )
					{
						if ( match.length == 1 )	// one space, preserve it
							return '&nbsp;' ;
						else if ( !offset )		// beginning of block
							return CKEDITOR.tools.repeat( '&nbsp;', match.length - 1 ) + ' ';
						else				// end of block
							return ' ' + CKEDITOR.tools.repeat( '&nbsp;', match.length - 1 );
					} ) ;

			// 3. Convert \n to <BR>.
			// 4. Convert contiguous (i.e. non-singular) spaces or tabs to &nbsp;
			 blockHtml =  blockHtml.replace( /\n/g, '<br>' ) ;
			 blockHtml =  blockHtml.replace( /[ \t]{2,}/g,
					function ( match )
					{
						return CKEDITOR.tools.repeat( '&nbsp;', match.length - 1 ) + ' ' ;
					} ) ;

			var newBlockClone = newBlock.clone();
			newBlockClone.setHtml(  blockHtml );
			docFrag.append( newBlockClone );
		}
		return docFrag;
	}

	/**
	 * Converting from a non-PRE block to a PRE block in formatting operations.
	 */
	function toPre( block, newBlock )
	{
		// First trim the block content.
		var preHtml = block.getHtml();

		// 1. Trim head/tail spaces, they're not visible.
		preHtml = replace( preHtml, /(?:^[ \t\n\r]+)|(?:[ \t\n\r]+$)/g, '' );
		// 2. Delete ANSI whitespaces immediately before and after <BR> because
		//    they are not visible.
		preHtml = preHtml.replace( /[ \t\r\n]*(<br[^>]*>)[ \t\r\n]*/gi, '$1' );
		// 3. Compress other ANSI whitespaces since they're only visible as one
		//    single space previously.
		// 4. Convert &nbsp; to spaces since &nbsp; is no longer needed in <PRE>.
		preHtml = preHtml.replace( /([ \t\n\r]+|&nbsp;)/g, ' ' );
		// 5. Convert any <BR /> to \n. This must not be done earlier because
		//    the \n would then get compressed.
		preHtml = preHtml.replace( /<br\b[^>]*>/gi, '\n' );

		// Krugle: IE normalizes innerHTML to <pre>, breaking whitespaces.
		if ( CKEDITOR.env.ie )
		{
			var temp = block.getDocument().createElement( 'div' );
			temp.append( newBlock );
			/**
			 * Copy pre attributes
			 * @author MindTouch
			 */
			newBlock.copyAttributes( temp );
			/* END */
			newBlock.$.outerHTML =  '<pre>' + preHtml + '</pre>';
			newBlock = temp.getFirst().remove();
			/**
			 * Copy pre attributes
			 * @author MindTouch
			 */
			temp.copyAttributes( newBlock );
			/* END */
		}
		else
			newBlock.setHtml( preHtml );

		return newBlock;
	}

	// Removes a style from an element itself, don't care about its subtree.
	function removeFromElement( style, element )
	{
		var def = style._.definition,
			attributes = CKEDITOR.tools.extend( {}, def.attributes, getOverrides( style )[ element.getName() ] ),
			styles = def.styles,
			// If the style is only about the element itself, we have to remove the element.
			removeEmpty = CKEDITOR.tools.isEmpty( attributes ) && CKEDITOR.tools.isEmpty( styles );

		// Remove definition attributes/style from the elemnt.
		for ( var attName in attributes )
		{
			// The 'class' element value must match (#1318).
			if ( ( attName == 'class' || style._.definition.fullMatch )
				&& element.getAttribute( attName ) != normalizeProperty( attName, attributes[ attName ] ) )
				continue;
			removeEmpty = element.hasAttribute( attName );
			element.removeAttribute( attName );
		}

		for ( var styleName in styles )
		{
			// Full match style insist on having fully equivalence. (#5018)
			if ( style._.definition.fullMatch
				&& element.getStyle( styleName ) != normalizeProperty( styleName, styles[ styleName ], true ) )
				continue;

			removeEmpty = removeEmpty || !!element.getStyle( styleName );
			element.removeStyle( styleName );
		}

		removeEmpty && removeNoAttribsElement( element );
	}

	// Removes a style from inside an element.
	function removeFromInsideElement( style, element )
	{
		var def = style._.definition,
			attribs = def.attributes,
			styles = def.styles,
			overrides = getOverrides( style );

		var innerElements = element.getElementsByTag( style.element );

		for ( var i = innerElements.count(); --i >= 0 ; )
			removeFromElement( style,  innerElements.getItem( i ) );

		// Now remove any other element with different name that is
		// defined to be overriden.
		for ( var overrideElement in overrides )
		{
			if ( overrideElement != style.element )
			{
				innerElements = element.getElementsByTag( overrideElement ) ;
				for ( i = innerElements.count() - 1 ; i >= 0 ; i-- )
				{
					var innerElement = innerElements.getItem( i );
					removeOverrides( innerElement, overrides[ overrideElement ] ) ;
				}
			}
		}

	}

	/**
	 *  Remove overriding styles/attributes from the specific element.
	 *  Note: Remove the element if no attributes remain.
	 * @param {Object} element
	 * @param {Object} overrides
	 */
	function removeOverrides( element, overrides )
	{
		var attributes = overrides && overrides.attributes ;

		if ( attributes )
		{
			for ( var i = 0 ; i < attributes.length ; i++ )
			{
				var attName = attributes[i][0], actualAttrValue ;

				if ( ( actualAttrValue = element.getAttribute( attName ) ) )
				{
					var attValue = attributes[i][1] ;

					// Remove the attribute if:
					//    - The override definition value is null ;
					//    - The override definition valie is a string that
					//      matches the attribute value exactly.
					//    - The override definition value is a regex that
					//      has matches in the attribute value.
					if ( attValue === null ||
							( attValue.test && attValue.test( actualAttrValue ) ) ||
							( typeof attValue == 'string' && actualAttrValue == attValue ) )
						element.removeAttribute( attName ) ;
				}
			}
		}

		removeNoAttribsElement( element );
	}

	// If the element has no more attributes, remove it.
	function removeNoAttribsElement( element )
	{
		// If no more attributes remained in the element, remove it,
		// leaving its children.
		if ( !element.hasAttributes() )
		{
			// Removing elements may open points where merging is possible,
			// so let's cache the first and last nodes for later checking.
			var firstChild	= element.getFirst();
			var lastChild	= element.getLast();

			element.remove( true );

			if ( firstChild )
			{
				// Check the cached nodes for merging.
				firstChild.type == CKEDITOR.NODE_ELEMENT && firstChild.mergeSiblings();

				if ( lastChild && !firstChild.equals( lastChild )
					&& lastChild.type == CKEDITOR.NODE_ELEMENT  )
					lastChild.mergeSiblings();
			}
		}
	}

	function getElement( style, targetDocument )
	{
		var el;

		var def = style._.definition;

		var elementName = style.element;

		// The "*" element name will always be a span for this function.
		if ( elementName == '*' )
			elementName = 'span';

		// Create the element.
		el = new CKEDITOR.dom.element( elementName, targetDocument );

		return setupElement( el, style );
	}

	function setupElement( el, style )
	{
		var def = style._.definition;
		var attributes = def.attributes;
		var styles = CKEDITOR.style.getStyleText( def );

		// Assign all defined attributes.
		if ( attributes )
		{
			for ( var att in attributes )
			{
				el.setAttribute( att, attributes[ att ] );
			}
		}

		// Assign all defined styles.
		if ( styles )
			el.setAttribute( 'style', styles );

		return el;
	}

	var varRegex = /#\((.+?)\)/g;
	function replaceVariables( list, variablesValues )
	{
		for ( var item in list )
		{
			list[ item ] = list[ item ].replace( varRegex, function( match, varName )
				{
					return variablesValues[ varName ];
				});
		}
	}


	// Returns an object that can be used for style matching comparison.
	// Attributes names and values are all lowercased, and the styles get
	// merged with the style attribute.
	function getAttributesForComparison( styleDefinition )
	{
		// If we have already computed it, just return it.
		var attribs = styleDefinition._AC;
		if ( attribs )
			return attribs;

		attribs = {};

		var length = 0;

		// Loop through all defined attributes.
		var styleAttribs = styleDefinition.attributes;
		if ( styleAttribs )
		{
			for ( var styleAtt in styleAttribs )
			{
				length++;
				attribs[ styleAtt ] = styleAttribs[ styleAtt ];
			}
		}

		// Includes the style definitions.
		var styleText = CKEDITOR.style.getStyleText( styleDefinition );
		if ( styleText )
		{
			if ( !attribs[ 'style' ] )
				length++;
			attribs[ 'style' ] = styleText;

			/**
			 * Style attribute protection
			 * @author MindTouch
			 */
			attribs[ '_cke_saved_style' ] = styleText;
			/* END */
		}

		// Appends the "length" information to the object.
		attribs._length = length;

		// Return it, saving it to the next request.
		return ( styleDefinition._AC = attribs );
	}

	/**
	 * Get the the collection used to compare the elements and attributes,
	 * defined in this style overrides, with other element. All information in
	 * it is lowercased.
	 * @param {CKEDITOR.style} style
	 */
	function getOverrides( style )
	{
		if ( style._.overrides )
			return style._.overrides;

		var overrides = ( style._.overrides = {} ),
			definition = style._.definition.overrides;

		if ( definition )
		{
			// The override description can be a string, object or array.
			// Internally, well handle arrays only, so transform it if needed.
			if ( !CKEDITOR.tools.isArray( definition ) )
				definition = [ definition ];

			// Loop through all override definitions.
			for ( var i = 0 ; i < definition.length ; i++ )
			{
				var override = definition[i];
				var elementName;
				var overrideEl;
				var attrs;

				// If can be a string with the element name.
				if ( typeof override == 'string' )
					elementName = override.toLowerCase();
				// Or an object.
				else
				{
					elementName = override.element ? override.element.toLowerCase() : style.element;
					attrs = override.attributes;
				}

				// We can have more than one override definition for the same
				// element name, so we attempt to simply append information to
				// it if it already exists.
				overrideEl = overrides[ elementName ] || ( overrides[ elementName ] = {} );

				if ( attrs )
				{
					// The returning attributes list is an array, because we
					// could have different override definitions for the same
					// attribute name.
					var overrideAttrs = ( overrideEl.attributes = overrideEl.attributes || new Array() );
					for ( var attName in attrs )
					{
						// Each item in the attributes array is also an array,
						// where [0] is the attribute name and [1] is the
						// override value.
						overrideAttrs.push( [ attName.toLowerCase(), attrs[ attName ] ] );
					}
				}
			}
		}

		return overrides;
	}

	function normalizeProperty( name, value, isStyle )
	{
		var temp = new CKEDITOR.dom.element( 'span' );
		temp [ isStyle ? 'setStyle' : 'setAttribute' ]( name, value );
		return temp[ isStyle ? 'getStyle' : 'getAttribute' ]( name );
	}

	function normalizeCssText( unparsedCssText, nativeNormalize )
	{
		var styleText;
		if ( nativeNormalize !== false )
		{
			// Injects the style in a temporary span object, so the browser parses it,
			// retrieving its final format.
			var temp = new CKEDITOR.dom.element( 'span' );
			temp.setAttribute( 'style', unparsedCssText );
			/**
			 * Style attribute protection
			 * getAttribute will return protected value
			 * @author MindTouch
			 */
			styleText = temp.getStyleAttribute( true ) || '';
			// styleText = temp.getAttribute( 'style' );
			/* END */
		}
		else
			styleText = unparsedCssText;

		// Shrinking white-spaces around colon and semi-colon (#4147).
		// Compensate tail semi-colon.
		return styleText.replace( /\s*([;:])\s*/, '$1' )
							 .replace( /([^\s;])$/, '$1;')
							 .replace( /,\s+/g, ',' ) // Trimming spaces after comma (e.g. font-family name)(#4107).
							 .toLowerCase();
	}

	// Turn inline style text properties into one hash.
	function parseStyleText( styleText )
	{
		var retval = {};
		styleText
		   .replace( /&quot;/g, '"' )
		   .replace( /\s*([^ :;]+)\s*:\s*([^;]+)\s*(?=;|$)/g, function( match, name, value )
		{
			retval[ name ] = value;
		} );
		return retval;
	}

	function compareCssText( source, target )
	{
		typeof source == 'string' && ( source = parseStyleText( source ) );
		typeof target == 'string' && ( target = parseStyleText( target ) );
		for( var name in source )
		{
			// Value 'inherit'  is treated as a wildcard,
			// which will match any value.
			if ( !( name in target &&
					( target[ name ] == source[ name ]
						|| source[ name ] == 'inherit'
						|| target[ name ] == 'inherit' ) ) )
			{
				return false;
			}
		}
		return true;
	}

	function applyStyle( document, remove )
	{
		// Get all ranges from the selection.
		var selection = document.getSelection();
		var ranges = selection.getRanges();
		var func = remove ? this.removeFromRange : this.applyToRange;

		// Apply the style to the ranges.
		for ( var i = 0 ; i < ranges.length ; i++ )
			func.call( this, ranges[ i ] );

		// Select the ranges again.
		selection.selectRanges( ranges );
	}
})();

CKEDITOR.styleCommand = function( style )
{
	this.style = style;
};

CKEDITOR.styleCommand.prototype.exec = function( editor )
{
	editor.focus();

	var doc = editor.document;

	if ( doc )
	{
		if ( this.state == CKEDITOR.TRISTATE_OFF )
			this.style.apply( doc );
		else if ( this.state == CKEDITOR.TRISTATE_ON )
			this.style.remove( doc );
	}

	return !!doc;
};

CKEDITOR.stylesSet = new CKEDITOR.resourceManager( '', 'stylesSet' );

// Backward compatibility (#5025).
CKEDITOR.addStylesSet = CKEDITOR.tools.bind( CKEDITOR.stylesSet.add, CKEDITOR.stylesSet );
CKEDITOR.loadStylesSet = function( name, url, callback )
	{
		CKEDITOR.stylesSet.addExternal( name, url, '' );
		CKEDITOR.stylesSet.load( name, callback );
	};


/**
 * Gets the current styleSet for this instance
 * @param {Function} The function to be called with the styles data.
 * @example
 * editor.getStylesSet( function( stylesDefinitions ) {} );
 */
CKEDITOR.editor.prototype.getStylesSet = function( callback )
{
	if ( !this._.stylesDefinitions )
	{
		var editor = this,
			// Respect the backwards compatible definition entry
			configStyleSet = editor.config.stylesCombo_stylesSet || editor.config.stylesSet || 'default';

		// #5352 Allow to define the styles directly in the config object
		if ( configStyleSet instanceof Array )
		{
			editor._.stylesDefinitions = configStyleSet;
			callback( configStyleSet );
			return;
		}

		var	partsStylesSet = configStyleSet.split( ':' ),
			styleSetName = partsStylesSet[ 0 ],
			externalPath = partsStylesSet[ 1 ],
			pluginPath = CKEDITOR.plugins.registered.styles.path;

		CKEDITOR.stylesSet.addExternal( styleSetName,
				externalPath ?
					partsStylesSet.slice( 1 ).join( ':' ) :
					pluginPath + 'styles/' + styleSetName + '.js', '' );

		CKEDITOR.stylesSet.load( styleSetName, function( stylesSet )
			{
				editor._.stylesDefinitions = stylesSet[ styleSetName ];
				callback( editor._.stylesDefinitions );
			} ) ;
	}
	else
		callback( this._.stylesDefinitions );
};

/**
 * The "styles definition set" to use in the editor. They will be used in the
 * styles combo and the Style selector of the div container. <br>
 * The styles may be defined in the page containing the editor, or can be
 * loaded on demand from an external file. In the second case, if this setting
 * contains only a name, the styles definition file will be loaded from the
 * "styles" folder inside the styles plugin folder.
 * Otherwise, this setting has the "name:url" syntax, making it
 * possible to set the URL from which loading the styles file.<br>
 * Previously this setting was available as config.stylesCombo_stylesSet<br>
 * Previously this setting was available as config.stylesCombo_stylesSet<br>
 * @type String|Array
 * @default 'default'
 * @since 3.3
 * @example
 * // Load from the styles' styles folder (mystyles.js file).
 * config.stylesSet = 'mystyles';
 * @example
 * // Load from a relative URL.
 * config.stylesSet = 'mystyles:/editorstyles/styles.js';
 * @example
 * // Load from a full URL.
 * config.stylesSet = 'mystyles:http://www.example.com/editorstyles/styles.js';
 * @example
 * // Load from a list of definitions.
 * config.stylesSet = [
 *  { name : 'Strong Emphasis', element : 'strong' },
 * { name : 'Emphasis', element : 'em' }, ... ];
 */
