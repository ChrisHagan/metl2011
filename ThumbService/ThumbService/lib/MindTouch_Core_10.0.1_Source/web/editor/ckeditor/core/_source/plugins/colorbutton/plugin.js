﻿/*
Copyright (c) 2003-2010, CKSource - Frederico Knabben. All rights reserved.
For licensing, see LICENSE.html or http://ckeditor.com/license
*/

CKEDITOR.plugins.add( 'colorbutton',
{
	requires : [ 'panelbutton', 'floatpanel', 'styles' ],

	init : function( editor )
	{
		var config = editor.config,
			lang = editor.lang.colorButton;

		var clickFn;

		if ( !CKEDITOR.env.hc )
		{
			addButton( 'TextColor', 'fore', lang.textColorTitle );
			addButton( 'BGColor', 'back', lang.bgColorTitle );
		}

		function addButton( name, type, title )
		{
			editor.ui.add( name, CKEDITOR.UI_PANELBUTTON,
				{
					label : title,
					title : title,
					className : 'cke_button_' + name.toLowerCase(),
					modes : { wysiwyg : 1 },

					panel :
					{
						css : editor.skin.editor.css,
						attributes : { role : 'listbox', 'aria-label' : lang.panelTitle }
					},

					onBlock : function( panel, block )
					{
						block.autoSize = true;
						block.element.addClass( 'cke_colorblock' );
						block.element.setHtml( renderColors( panel, type ) );

						var keys = block.keys;
						keys[ 39 ]	= 'next';					// ARROW-RIGHT
						keys[ 40 ]	= 'next';					// ARROW-DOWN
						keys[ 9 ]	= 'next';					// TAB
						keys[ 37 ]	= 'prev';					// ARROW-LEFT
						keys[ 38 ]	= 'prev';					// ARROW-UP
						keys[ CKEDITOR.SHIFT + 9 ]	= 'prev';	// SHIFT + TAB
						keys[ 32 ]	= 'click';					// SPACE
					}
				});
		}


		function renderColors( panel, type )
		{
			var output = [],
				colors = config.colorButton_colors.split( ',' ),
				total = colors.length + ( config.colorButton_enableMore ? 2 : 1 );

			var clickFn = CKEDITOR.tools.addFunction( function( color, type )
				{
					if ( color == '?' )
					{
						var applyColorStyle = arguments.callee;
						function onColorDialogClose( evt )
						{
							this.removeListener( 'ok', onColorDialogClose );
							this.removeListener( 'cancel', onColorDialogClose );

							evt.name == 'ok' && applyColorStyle( this.getContentElement( 'picker', 'selectedColor' ).getValue(), type );
						}

						editor.openDialog( 'colordialog', function()
						{
							this.on( 'ok', onColorDialogClose );
							this.on( 'cancel', onColorDialogClose );
						} );

						return;
					}

					editor.focus();

					panel.hide();


					editor.fire( 'saveSnapshot' );

					// Clean up any conflicting style within the range.
					new CKEDITOR.style( config['colorButton_' + type + 'Style'], { color : 'inherit' } ).remove( editor.document );

					/**
					 * Apply background color to the cell
					 * @see #0004187
					 * @author MindTouch
					 */
					var cells = [], i;

					if ( type == 'back' )
					{
						var selection = editor.getSelection(),
							range = selection && selection.getRanges()[0];

						cells = CKEDITOR.plugins.tabletools.getSelectedCells( selection );

						// if text is selected into cell then apply bgcolor to selected text
						// instead of cell
						if ( !range || ( cells.length == 1 && !range.collapsed ) )
							cells = [];
					}

					if ( cells.length )
					{
						for ( i = 0 ; i < cells.length ; i++ )
						{
							if ( color )
							{
								cells[i].setStyle( 'background-color', color );
							}
							else
							{
								cells[i].removeStyle( 'background-color' );
							}
						}
					}
					else if ( color )
					/* END */
					{
						var colorStyle = config['colorButton_' + type + 'Style'];

						colorStyle.childRule = type == 'back' ?
							// It's better to apply background color as the innermost style. (#3599)
							function(){ return false; } :
							// Fore color style must be applied inside links instead of around it.
							function( element ){ return element.getName() != 'a'; };

						new CKEDITOR.style( colorStyle, { color : color } ).apply( editor.document );
					}

					editor.fire( 'saveSnapshot' );
				});

			// Render the "Automatic" button.
			output.push(
				'<a class="cke_colorauto" _cke_focus=1 hidefocus=true' +
					' title="', lang.auto, '"' +
					' onclick="CKEDITOR.tools.callFunction(', clickFn, ',null,\'', type, '\');return false;"' +
					' href="javascript:void(\'', lang.auto, '\')"' +
					' role="option" aria-posinset="1" aria-setsize="', total, '">' +
					'<table role="presentation" cellspacing=0 cellpadding=0 width="100%">' +
						'<tr>' +
							'<td>' +
								'<span class="cke_colorbox" style="background-color:#000"></span>' +
							'</td>' +
							'<td colspan=7 align=center>',
								lang.auto,
							'</td>' +
						'</tr>' +
					'</table>' +
				'</a>' +
				'<table role="presentation" cellspacing=0 cellpadding=0 width="100%">' );

			// Render the color boxes.
			for ( var i = 0 ; i < colors.length ; i++ )
			{
				if ( ( i % 8 ) === 0 )
					output.push( '</tr><tr>' );

				var parts = colors[ i ].split( '/' ),
					colorName = parts[ 0 ],
					colorCode = parts[ 1 ] || colorName;

				// The data can be only a color code (without #) or colorName + color code
				// If only a color code is provided, then the colorName is the color with the hash
				// Convert the color from RGB to RRGGBB for better compatibility with IE and <font>. See #5676
				if (!parts[1])
					colorName = '#' + colorName.replace( /^(.)(.)(.)$/, '$1$1$2$2$3$3' );

				var colorLabel = editor.lang.colors[ colorCode ] || colorCode;
				output.push(
					'<td>' +
						'<a class="cke_colorbox" _cke_focus=1 hidefocus=true' +
							' title="', colorLabel, '"' +
							' onclick="CKEDITOR.tools.callFunction(', clickFn, ',\'', colorName, '\',\'', type, '\'); return false;"' +
							' href="javascript:void(\'', colorLabel, '\')"' +
							' role="option" aria-posinset="', ( i + 2 ), '" aria-setsize="', total, '">' +
							'<span class="cke_colorbox" style="background-color:#', colorCode, '"></span>' +
						'</a>' +
					'</td>' );
			}

			// Render the "More Colors" button.
			if ( config.colorButton_enableMore )
			{
				output.push(
					'</tr>' +
					'<tr>' +
						'<td colspan=8 align=center>' +
							'<a class="cke_colormore" _cke_focus=1 hidefocus=true' +
								' title="', lang.more, '"' +
								' onclick="CKEDITOR.tools.callFunction(', clickFn, ',\'?\',\'', type, '\');return false;"' +
								' href="javascript:void(\'', lang.more, '\')"',
								' role="option" aria-posinset="', total, '" aria-setsize="', total, '">',
								lang.more,
							'</a>' +
						'</td>' );	// It is later in the code.
			}

			output.push( '</tr></table>' );

			return output.join( '' );
		}
	}
});

/**
 * Whether to enable the "More Colors..." button in the color selectors.
 * @default false
 * @type Boolean
 * @example
 * config.colorButton_enableMore = false;
 */
CKEDITOR.config.colorButton_enableMore = true;

/**
 * Defines the colors to be displayed in the color selectors. It's a string
 * containing the hexadecimal notation for HTML colors, without the "#" prefix.
 *
 * Since 3.3: A name may be optionally defined by prefixing the entries with the
 * name and the slash character. For example, "FontColor1/FF9900" will be
 * displayed as the color #FF9900 in the selector, but will be outputted as "FontColor1".
 * @type String
 * @default '000,800000,8B4513,2F4F4F,008080,000080,4B0082,696969,B22222,A52A2A,DAA520,006400,40E0D0,0000CD,800080,808080,F00,FF8C00,FFD700,008000,0FF,00F,EE82EE,A9A9A9,FFA07A,FFA500,FFFF00,00FF00,AFEEEE,ADD8E6,DDA0DD,D3D3D3,FFF0F5,FAEBD7,FFFFE0,F0FFF0,F0FFFF,F0F8FF,E6E6FA,FFF'
 * @example
 * // Brazil colors only.
 * config.colorButton_colors = '00923E,F8C100,28166F';
 * @example
 * config.colorButton_colors = 'FontColor1/FF9900,FontColor2/0066CC,FontColor3/F00'
 */
CKEDITOR.config.colorButton_colors =
	'000,800000,8B4513,2F4F4F,008080,000080,4B0082,696969,' +
	'B22222,A52A2A,DAA520,006400,40E0D0,0000CD,800080,808080,' +
	'F00,FF8C00,FFD700,008000,0FF,00F,EE82EE,A9A9A9,' +
	'FFA07A,FFA500,FFFF00,00FF00,AFEEEE,ADD8E6,DDA0DD,D3D3D3,' +
	'FFF0F5,FAEBD7,FFFFE0,F0FFF0,F0FFFF,F0F8FF,E6E6FA,FFF';

/**
 * Holds the style definition to be used to apply the text foreground color.
 * @type Object
 * @example
 * // This is basically the default setting value.
 * config.colorButton_foreStyle =
 *     {
 *         element : 'span',
 *         styles : { 'color' : '#(color)' }
 *     };
 */
CKEDITOR.config.colorButton_foreStyle =
	{
		element		: 'span',
		styles		: { 'color' : '#(color)' },
		overrides	: [ { element : 'font', attributes : { 'color' : null } } ]
	};

/**
 * Holds the style definition to be used to apply the text background color.
 * @type Object
 * @example
 * // This is basically the default setting value.
 * config.colorButton_backStyle =
 *     {
 *         element : 'span',
 *         styles : { 'background-color' : '#(color)' }
 *     };
 */
CKEDITOR.config.colorButton_backStyle =
	{
		element		: 'span',
		styles		: { 'background-color' : '#(color)' }
	};
