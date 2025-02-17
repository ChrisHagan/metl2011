<?php
/*
 * FCKeditor - The text editor for Internet - http://www.fckeditor.net
 * Copyright (C) 2003-2009 Frederico Caldeira Knabben
 *
 * == BEGIN LICENSE ==
 *
 * Licensed under the terms of any of the following licenses at your
 * choice:
 *
 *  - GNU General Public License Version 2 or later (the "GPL")
 *    http://www.gnu.org/licenses/gpl.html
 *
 *  - GNU Lesser General Public License Version 2.1 or later (the "LGPL")
 *    http://www.gnu.org/licenses/lgpl.html
 *
 *  - Mozilla Public License Version 1.1 or later (the "MPL")
 *    http://www.mozilla.org/MPL/MPL-1.1.html
 *
 * == END LICENSE ==
 *
 * This is the File Manager Connector for PHP.
 */

function GetFolders( $resourceType, $currentFolder )
{
	$sPage = '';
	
	switch ( $currentFolder )
	{
		case '/':
			$sPage = 'home';
			break;
		default:
			$sPage = '=' . trim($currentFolder, '/');
	}
	
	$Plug = DekiPlug::getInstance();
	$Result = $Plug->At('pages', $sPage, 'subpages')->Get();
	
	$aSubpages = $Result->getAll('body/subpages/page.subpage');

	// Open the "Folders" node.
	echo "<Folders>" ;

	foreach ( $aSubpages as $aSubpage )
	{
		$oSubPage = new DekiResult( $aSubpage ) ;
		echo '<Folder name="' . ConvertToXmlAttribute( $oSubPage->getVal('title') ) . '" />' ;
	}

	// Close the "Folders" node.
	echo "</Folders>" ;
}

function GetFoldersAndFiles( $resourceType, $currentFolder )
{
	$sPage = '';
	
	switch ( $currentFolder )
	{
		case '/':
			$sPage = 'home';
			break;
		default:
			$sPage = '=' . trim($currentFolder, '/');
	}
	
	$Plug = DekiPlug::getInstance();
	$Result = $Plug->At('pages', $sPage)->AtRaw('files,subpages')->Get();
	
	$aSubpages = $Result->getAll('body/page/subpages/page.subpage');
	$aFiles = $Result->getAll('body/page/files/file');
	
	// Send the folders
	echo '<Folders>' ;

	foreach ( $aSubpages as $aSubpage )
	{
		$oSubPage = new DekiResult( $aSubpage );
		echo '<Folder name="' . ConvertToXmlAttribute( $oSubPage->getVal('title') ) . '" />' ;
	}

	echo '</Folders>' ;

	// Send the files
	echo '<Files>' ;

	foreach ( $aFiles as $aFile )
	{
		$oFile = new DekiResult( $aFile ) ;
		
		$sType = $oFile->getVal('contents/@type') ;
		
		switch ( $resourceType )
		{
			case 'Image':
				if ( strncasecmp($resourceType, $sType, strlen($resourceType)) != 0 )
					continue 2 ;
				break ;
			case 'Flash':
				if ( stripos($sType, $resourceType) === false )
					continue 2 ;
				break ;
		}
		
		$iFileSize = (int) $oFile->getVal('contents/@size') ;
		
		if ( !$iFileSize )
		{
			$iFileSize = 0 ;
		}
		
		if ( $iFileSize > 0 )
		{
			$iFileSize = round( $iFileSize / 1024 ) ;
			if ( $iFileSize < 1 ) $iFileSize = 1 ;
		}
		
		echo '<File type="' . ConvertToXmlAttribute($sType) . '" name="' . ConvertToXmlAttribute( $oFile->getVal('filename') ) . '" size="' . $iFileSize . '" />' ;
	}
	
	echo '</Files>' ;
}

function CreateFolder( $resourceType, $currentFolder )
{
	if (!isset($_GET)) {
		global $_GET;
	}
	$sErrorNumber	= '0' ;
	$sErrorMsg		= '' ;

	if ( isset( $_GET['NewFolderName'] ) )
	{
		$sNewFolderName = $_GET['NewFolderName'] ;
		$sNewFolderName = SanitizeFolderName( $sNewFolderName ) ;

		if ( strpos( $sNewFolderName, '..' ) !== FALSE )
			$sErrorNumber = '102' ;		// Invalid folder name.
		else
		{
			// Map the virtual path to the local server path of the current folder.
			$sServerDir = ServerMapFolder( $resourceType, $currentFolder, 'CreateFolder' ) ;

			if ( is_writable( $sServerDir ) )
			{
				$sServerDir .= $sNewFolderName ;

				$sErrorMsg = CreateServerFolder( $sServerDir ) ;

				switch ( $sErrorMsg )
				{
					case '' :
						$sErrorNumber = '0' ;
						break ;
					case 'Invalid argument' :
					case 'No such file or directory' :
						$sErrorNumber = '102' ;		// Path too long.
						break ;
					default :
						$sErrorNumber = '110' ;
						break ;
				}
			}
			else
				$sErrorNumber = '103' ;
		}
	}
	else
		$sErrorNumber = '102' ;

	// Create the "Error" node.
	echo '<Error number="' . $sErrorNumber . '" />' ;
}

function FileUpload( $resourceType, $currentFolder, $sCommand )
{
	if (!isset($_FILES)) {
		global $_FILES;
	}
	$sErrorNumber = '0' ;
	$sFileName = '' ;

	if ( isset( $_FILES['NewFile'] ) && !is_null( $_FILES['NewFile']['tmp_name'] ) )
	{
		global $Config ;

		$oFile = $_FILES['NewFile'] ;

		// Map the virtual path to the local server path.
		$sServerDir = ServerMapFolder( $resourceType, $currentFolder, $sCommand ) ;

		// Get the uploaded file name.
		$sFileName = $oFile['name'] ;
		$sFileName = SanitizeFileName( $sFileName ) ;

		$sOriginalFileName = $sFileName ;

		// Get the extension.
		$sExtension = substr( $sFileName, ( strrpos($sFileName, '.') + 1 ) ) ;
		$sExtension = strtolower( $sExtension ) ;

		if ( isset( $Config['SecureImageUploads'] ) )
		{
			if ( ( $isImageValid = IsImageValid( $oFile['tmp_name'], $sExtension ) ) === false )
			{
				$sErrorNumber = '202' ;
			}
		}

		if ( isset( $Config['HtmlExtensions'] ) )
		{
			if ( !IsHtmlExtension( $sExtension, $Config['HtmlExtensions'] ) &&
				( $detectHtml = DetectHtml( $oFile['tmp_name'] ) ) === true )
			{
				$sErrorNumber = '202' ;
			}
		}

		// Check if it is an allowed extension.
		if ( !$sErrorNumber && IsAllowedExt( $sExtension, $resourceType ) )
		{
			$iCounter = 0 ;

			while ( true )
			{
				$sFilePath = $sServerDir . $sFileName ;

				if ( is_file( $sFilePath ) )
				{
					$iCounter++ ;
					$sFileName = RemoveExtension( $sOriginalFileName ) . '(' . $iCounter . ').' . $sExtension ;
					$sErrorNumber = '201' ;
				}
				else
				{
					move_uploaded_file( $oFile['tmp_name'], $sFilePath ) ;

					if ( is_file( $sFilePath ) )
					{
						if ( isset( $Config['ChmodOnUpload'] ) && !$Config['ChmodOnUpload'] )
						{
							break ;
						}

						$permissions = 0777;

						if ( isset( $Config['ChmodOnUpload'] ) && $Config['ChmodOnUpload'] )
						{
							$permissions = $Config['ChmodOnUpload'] ;
						}

						$oldumask = umask(0) ;
						chmod( $sFilePath, $permissions ) ;
						umask( $oldumask ) ;
					}

					break ;
				}
			}

			if ( file_exists( $sFilePath ) )
			{
				//previous checks failed, try once again
				if ( isset( $isImageValid ) && $isImageValid === -1 && IsImageValid( $sFilePath, $sExtension ) === false )
				{
					@unlink( $sFilePath ) ;
					$sErrorNumber = '202' ;
				}
				else if ( isset( $detectHtml ) && $detectHtml === -1 && DetectHtml( $sFilePath ) === true )
				{
					@unlink( $sFilePath ) ;
					$sErrorNumber = '202' ;
				}
			}
		}
		else
			$sErrorNumber = '202' ;
	}
	else
		$sErrorNumber = '202' ;


	$sFileUrl = CombinePaths( GetResourceTypePath( $resourceType, $sCommand ) , $currentFolder ) ;
	$sFileUrl = CombinePaths( $sFileUrl, $sFileName ) ;

	SendUploadResults( $sErrorNumber, $sFileUrl, $sFileName ) ;

	exit ;
}
