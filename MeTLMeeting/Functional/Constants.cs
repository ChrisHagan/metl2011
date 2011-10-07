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
    }

    class ErrorMessages
    {
        public const string PROBLEM_SHUTTING_DOWN = "MeTL did not shutdown correctly.";
        public const string EXPECTED_MAIN_WINDOW = "Expected to find MeTL '" + Constants.ID_METL_MAIN_WINDOW + "'.";
        public const string UNABLE_TO_FIND_EXECUTABLE = "Unable to find the MeTL executable.";
    }
}
