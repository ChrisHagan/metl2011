﻿/*
Copyright (c) 2003-2010, CKSource - Frederico Knabben. All rights reserved.
For licensing, see LICENSE.html or http://ckeditor.com/license
*/

(function()
{
	CKEDITOR.plugins.add( 'stylescombo',
	{
		requires : [ 'richcombo', 'styles' ],

		init : function( editor )
		{
			var config = editor.config,
				lang = editor.lang.stylesCombo,
				styles = {},
				stylesList = [];

			function loadStylesSet( callback )
			{
				editor.getStylesSet( function( stylesDefinitions )
				{
					if ( !stylesList.length )
					{
						var style,
							styleName;

						// Put all styles into an Array.
						for ( var i = 0 ; i < stylesDefinitions.length ; i++ )
						{
							var styleDefinition = stylesDefinitions[ i ];

							styleName = styleDefinition.name;

							style = styles[ styleName ] = new CKEDITOR.style( styleDefinition );
							style._name = styleName;
							style._.enterMode = config.enterMode;

							stylesList.push( style );
						}

						// Sorts the Array, so the styles get grouped by type.
						stylesList.sort( sortStyles );
					}

					callback && callback();
				});
			}

			editor.ui.addRichCombo( 'Styles',
				{
					label : lang.label,
					title : lang.panelTitle,
					className : 'cke_styles',

					panel :
					{
						css : editor.skin.editor.css.concat( config.contentsCss ),
						multiSelect : true,
						attributes : { 'aria-label' : lang.panelTitle }
					},

					init : function()
					{
						var combo = this;

						loadStylesSet( function()
							{
								var style, styleName;

								// Loop over the Array, adding all items to the
								// combo.
								var lastType;
								for ( var i = 0 ; i < stylesList.length ; i++ )
								{
									style = stylesList[ i ];
									styleName = style._name;

									var type = style.type;

									if ( type != lastType )
									{
										combo.startGroup( lang[ 'panelTitle' + String( type ) ] );
										lastType = type;
									}

									combo.add(
										styleName,
										style.type == CKEDITOR.STYLE_OBJECT ? styleName : style.buildPreview(),
										styleName );
								}

								combo.commit();

								combo.onOpen();
							});
					},

					onClick : function( value )
					{
						editor.focus();
						editor.fire( 'saveSnapshot' );

						var style = styles[ value ],
							selection = editor.getSelection();

						var elementPath = new CKEDITOR.dom.elementPath( selection.getStartElement() );

						/**
						 * Allow to remove block style
						 * 
						 * @see #0007733
						 * @author MindTouch
						 * 
						 */
//						if ( style.type == CKEDITOR.STYLE_INLINE && style.checkActive( elementPath ) )
						if ( style.checkActive( elementPath ) )
						/* END */
							style.remove( editor.document );
						else
							style.apply( editor.document );

						editor.fire( 'saveSnapshot' );
					},

					onRender : function()
					{
						editor.on( 'selectionChange', function( ev )
							{
								var currentValue = this.getValue();

								var elementPath = ev.data.path,
									elements = elementPath.elements;

								// For each element into the elements path.
								for ( var i = 0, element ; i < elements.length ; i++ )
								{
									element = elements[i];

									// Check if the element is removable by any of
									// the styles.
									for ( var value in styles )
									{
										if ( styles[ value ].checkElementRemovable( element, true ) )
										{
											if ( value != currentValue )
												this.setValue( value );
											return;
										}
									}
								}

								// If no styles match, just empty it.
								this.setValue( '' );
							},
							this);
					},

					onOpen : function()
					{
						if ( CKEDITOR.env.ie || CKEDITOR.env.webkit )
							editor.focus();

						var selection = editor.getSelection();

						var element = selection.getSelectedElement(),
							elementPath = new CKEDITOR.dom.elementPath( element || selection.getStartElement() );

						var counter = [ 0, 0, 0, 0 ];
						this.showAll();
						this.unmarkAll();
						for ( var name in styles )
						{
							var style = styles[ name ],
								type = style.type;

							if ( style.checkActive( elementPath ) )
								this.mark( name );
							else if ( type == CKEDITOR.STYLE_OBJECT && !style.checkApplicable( elementPath ) )
							{
								this.hideItem( name );
								counter[ type ]--;
							}

							counter[ type ]++;
						}

						if ( !counter[ CKEDITOR.STYLE_BLOCK ] )
							this.hideGroup( lang[ 'panelTitle' + String( CKEDITOR.STYLE_BLOCK ) ] );

						if ( !counter[ CKEDITOR.STYLE_INLINE ] )
							this.hideGroup( lang[ 'panelTitle' + String( CKEDITOR.STYLE_INLINE ) ] );

						if ( !counter[ CKEDITOR.STYLE_OBJECT ] )
							this.hideGroup( lang[ 'panelTitle' + String( CKEDITOR.STYLE_OBJECT ) ] );

						/**
						 * Add class to each group list
						 * 
						 * @author MindTouch
						 */
						for ( var groupTitle in this._.list._.groups )
						{
							var groupId = this._.list._.groups[ groupTitle ],
								group = this._.list.element.getDocument().getById( groupId ),
								list = group && group.getNext(),
								listClass = 'cke_panel_group';

							switch ( groupTitle )
							{
								case lang[ 'panelTitle' + String( CKEDITOR.STYLE_BLOCK ) ]:
									listClass += String( CKEDITOR.STYLE_BLOCK );
									break;
								case lang[ 'panelTitle' + String( CKEDITOR.STYLE_INLINE ) ]:
									listClass += String( CKEDITOR.STYLE_INLINE );
									break;
								case lang[ 'panelTitle' + String( CKEDITOR.STYLE_OBJECT ) ]:
									listClass += String( CKEDITOR.STYLE_OBJECT );
									break;
							}

							if ( list && list.getName() == 'ul' )
							{
								list.addClass( listClass );
							}
						}
						/* END */
					}
				});

			editor.on( 'instanceReady', function() { loadStylesSet(); } );
		}
	});

	function sortStyles( styleA, styleB )
	{
		var typeA = styleA.type,
			typeB = styleB.type;

		return typeA == typeB ? 0 :
			typeA == CKEDITOR.STYLE_OBJECT ? -1 :
			typeB == CKEDITOR.STYLE_OBJECT ? 1 :
			typeB == CKEDITOR.STYLE_BLOCK ? 1 :
			-1;
	}
})();
