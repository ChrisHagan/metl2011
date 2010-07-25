using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Windows.Browser;

namespace SilverlightApplication1
{
    /// <summary>
    /// Provides managed access to the wave API in gadgets.
    /// </summary>
    /// <remarks>See: http://code.google.com/apis/wave/extensions/gadgets/reference.html for more information
    /// on the wave gadget API.</remarks>
    [ScriptableType]
    public class WaveManager
    {
        /// <summary>
        ///  Gadget state updated
        /// </summary>
        [ScriptableMember]
        public EventHandler StateUpdated { get; set; }

        /// <summary>
        /// Particicpants updated
        /// </summary>
        [ScriptableMember]
        public EventHandler ParticipantsUpdated { get; set; }

        private static WaveManager _wave;
        private static State _state;

        private WaveManager() { }

        [ScriptableMember()]
        public static WaveManager Wave
        {
            get
            {
                if (_wave == null)
                    _wave = new WaveManager();

                return _wave;
            }
        }

        [ScriptableMember]
        public State State
        {
            get
            {
                if (_state == null)
                    _state = new State();

                return _state;
            }
        }

        /// <summary>
        /// Indicates whether the gadget runs inside a wave container.
        /// </summary>
        /// <returns>True if the gadget runs inside a wave container. False otherwise.</returns>
        public bool IsInWaveContainer()
        {
            return Convert.ToInt32(HtmlPage.Window.Eval("wave.isInWaveContainer()")) == 1;
        }

        /// <summary>
        /// Returns the playback state of the wave/wavelet/gadget.
        /// </summary>
        /// <returns>Whether the gadget should be in the playback state</returns>
        public bool IsPlayback()
        {
            return Convert.ToInt32(HtmlPage.Window.Eval("wave.isPlayback()")) == 1;
        }

        /// <summary>
        /// Retrieves "gadget time" which is either the playback frame time in the playback mode or the current time otherwise.
        /// </summary>
        /// <returns>The gadget time.</returns>
        public DateTime GetTime()
        {
            var so = HtmlPage.Window.Eval("new Date(wave.getTime())") as ScriptObject;
            return so.ConvertTo<DateTime>();
        }

        /// <summary>
        /// Get the participant whose client renders this gadget.
        /// </summary>
        /// <returns>The viewer (null if not known)</returns>
        public Participant GetViewer()
        {
            var so = HtmlPage.Window.Eval("wave.getViewer()") as ScriptObject;

            if (so == null)
                return null;
            else
                return new Participant(so);
        }

        /// <summary>
        /// Get host, participant who added this gadget to the blip. 
        /// Note that the host may no longer be in the participant list.
        /// </summary>
        /// <returns>The host (null if not known)</returns>
        public Participant GetHost()
        {
            var so = HtmlPage.Window.Eval("wave.getHost()") as ScriptObject;

            if (so == null)
                return null;
            else
                return new Participant(so);
        }

        /// <summary>
        /// Returns a Participant with the given id.
        /// </summary>
        /// <param name="id">The id of the participant to retrieve.</param>
        /// <returns>The participant with the given id.</returns>
        public Participant GetParticipantById(string id)
        {
            var so = HtmlPage.Window.Eval("wave.getParticipantById('" + id + "')") as ScriptObject;

            if (so == null)
                return null;
            else
                return new Participant(so);
        }

        /// <summary>
        /// Returns a collection of participants on the Wave.
        /// </summary>
        /// <returns>Collection of wave participants</returns>
        public ObservableCollection<Participant> GetParticipants()
        {
            var so = HtmlPage.Window.Eval("wave.getParticipants()") as ScriptObject;
            var soa = so.ConvertTo<ScriptObject[]>();
            var participants = new ObservableCollection<Participant>();

            foreach (ScriptObject o in soa)
            {
                participants.Add(new Participant(o));
            }
            return participants;
        }
    }

    /// <summary>
    /// State class for managing the gadget state.
    /// </summary>
    [ScriptableType]
    public class State
    {
        /// <summary>
        /// Updates the state delta. This is an asynchronous call that will update the state and 
        /// not take effect immediately. Creating any key with a null value will attempt to delete the key.
        /// </summary>
        /// <param name="key">Key of state property to update</param>
        /// <param name="value">Value of state property to update</param>
        [ScriptableMember]
        public void SubmitDelta(string key, string value)
        {
            //HtmlPage.Window.Eval("wave.getState().submitDelta({'" + key + "':" + (String.IsNullOrEmpty(value) ? "null" : value) + "})");
            HtmlPage.Window.Eval("receiveSilverlightDelta('"+key+"','"+value+"')");
        }

        [ScriptableMember]
        public void inc()
        {
            HtmlPage.Window.Eval("buttonClicked()");
        }

        /// <summary>
        /// Updates the state delta. This is an asynchronous call that will update the state and 
        /// not take effect immediately. Creating any key with a null value will attempt to delete the key.
        /// </summary>
        /// <param name="kvp">Key-value pairs representing a delta of keys to update.</param>
        
        public void SubmitDelta(Dictionary<string, string> kvp)
        {
            foreach (string key in kvp.Keys)
            {
                SubmitDelta(key, kvp[key]);
            }
        }

        /// <summary>
        /// Retrieve a value from the synchronized state. 
        /// </summary>
        /// <param name="key">Value for the specified key to retrieve.</param>
        /// <returns>Object for the specified key or null if not found.</returns>
        /// <remarks>As of now, get always returns a string. This will change at some point 
        /// to return whatever was set.</remarks>
        [ScriptableMember]
        public string Get(string key)
        {
            return HtmlPage.Window.Eval("wave.getState().get('" + key + "')").ToString();
        }

        /// <summary>
        /// Retrieve a value from the synchronized state.
        /// </summary>
        /// <param name="key">Value for the specified key to retrieve.</param>
        /// <param name="defaultValue">Default value if nonexistent</param>
        /// <returns>Object for the specified key or null if not found.</returns>
        [ScriptableMember]
        public string Get(string key, string defaultValue)
        {
            return HtmlPage.Window.Eval("wave.getState().get('" + key + "','" + defaultValue + "')").ToString();
        }

        /// <summary>
        /// Returns a dictionary of all state key / values
        /// </summary>
        /// <returns>Dictionary of all state key / values</returns>
        public Dictionary<string, string> Get()
        {
            Dictionary<string, string> kvp = new Dictionary<string, string>();
            List<string> keys = GetKeys();
            foreach (string key in keys)
            {
                kvp[key] = Get(key);
            }
            return kvp;
        }

        /// <summary>
        /// Pretty prints the current state object. Note this is a debug method only.
        /// </summary>
        /// <returns>The stringified state</returns>
        [ScriptableMember]
        public override string ToString()
        {
            return HtmlPage.Window.Eval("wave.getState().toString()").ToString().Replace("\n", string.Empty);
        }

        /// <summary>
        /// Retrieve the valid keys for the synchronized state.
        /// </summary>
        /// <returns>List of keys</returns>
        [ScriptableMember]
        public List<string> GetKeys()
        {
            var so = HtmlPage.Window.Eval("wave.getState().getKeys()") as ScriptObject;
            var oa = so.ConvertTo<object[]>();
            List<string> keys = new List<string>();

            foreach (object o in oa)
            {
                keys.Add(o.ToString());
            }
            return keys;
        }
    }

    /// <summary>
    /// Participant class that describes participants on a wave.
    /// </summary>
    public class Participant
    {
        /// <summary>
        /// Unique identifier of this participant.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Display name of this participant.
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// The url of the thumbnail image for this participant.
        /// </summary>
        public string ThumbnailUrl { get; private set; }

        public Participant() { }
        public Participant(ScriptObject so)
        {
            if (so != null)
            {
                Id = so.GetProperty("id_").ToString();
                DisplayName = so.GetProperty("displayName_").ToString();
                ThumbnailUrl = so.GetProperty("thumbnailUrl_").ToString();
            }
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}