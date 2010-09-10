using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace PowerpointJabber
{
    class VLE_integration
    {
        private static string host = "https://madam.adm.monash.edu.au:1188/";
        public static string UploadPresentationToVLE()
        {
            var filename = ThisAddIn.instance.Application.ActivePresentation.Name;
            if (!filename.EndsWith(".ppt"))
            {
                var parts = filename.Split('.');
                string newFilename = ""; 
                for (int i = 0; i < parts.Count() - 1; i++)
                {
                    newFilename += parts[i];
                }
                filename = newFilename + ".ppt";
            } 
            ThisAddIn.instance.Application.ActivePresentation.SaveCopyAs(filename, Microsoft.Office.Interop.PowerPoint.PpSaveAsFileType.ppSaveAsPresentation, Microsoft.Office.Core.MsoTriState.msoTrue);
            try
            {
                var response = HttpResourceProvider.securePutFile(host + "upload_nested.yaws?path=PPTUpload/proofOfConcept&overwrite=true", filename);
                return response.ToString();
            }
            catch (Exception ex) { return ex.Message.ToString(); };
        }
    }
}
