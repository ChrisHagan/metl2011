using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Functional
{
    class Constants
    {
        public const string ID_METL_MAIN_WINDOW = "ribbonWindow";
        public const string ID_METL_CONVERSATION_SEARCH_TEXTBOX = "SearchInput";
        public const string ID_METL_VERSION_LABEL = "VersionLabel";
    }

    class ErrorMessages
    {
        public const string PROBLEM_SHUTTING_DOWN = "MeTL did not shutdown correctly.";
        public const string EXPECTED_MAIN_WINDOW = "Expected to find MeTL '" + Constants.ID_METL_MAIN_WINDOW + "'.";
        public const string UNABLE_TO_FIND_EXECUTABLE = "Unable to find the MeTL executable.";
        public const string EXPECTED_ONE_INSTANCE = "Expected only one instance of MeTL to be running.";
        public const string WAIT_FOR_CONTROL_FAILED = "WaitForControl function failed.";
        public const string VERSION_MISMATCH = "MeTL version does not match with provided.";
    }
}
