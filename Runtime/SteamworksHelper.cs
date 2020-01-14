
#if !UNITY_STANDALONE
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS

using System;
using System.Collections.Generic;
using Steamworks;

namespace UnityExtensions.Steamworks
{
    public static class CallbackEvent<T>
    {
        static Callback<T> _callback;
        static Action<T> _listeners;

        public static void AddListener(Action<T> listener)
        {
            _listeners += listener;

            if (_callback == null)
            {
                _callback = Callback<T>.Create(p => _listeners?.Invoke(p));
            }
        }

        public static void RemoveListener(Action<T> listener)
        {
            _listeners -= listener;
        }

    } // class CallbackEvent<T>


    public class CallResultEvent<T>
    {
        static Stack<CallResultEvent<T>> _inactive = new Stack<CallResultEvent<T>>(4);
        
        CallResult<T> _callResult;
        Action<T, bool> _listener;

        private CallResultEvent()
        {
            _callResult = CallResult<T>.Create();
        }

        void Callback(T param, bool bIOFailure)
        {
            _listener?.Invoke(param, bIOFailure);
            _listener = null;
            _callResult.Set(SteamAPICall_t.Invalid);
            SteamworksHelper._activeCallResultEvents.Remove(this);
            _inactive.Push(this);
        }

        /// <summary>
        /// Try monitor a SteamAPICall_t handle, reutrn false if the handle is invalid.
        /// </summary>
        public static bool TryMonitor(SteamAPICall_t handle, Action<T, bool> listener)
        {
            if (handle == SteamAPICall_t.Invalid) return false;

            CallResultEvent<T> @event;

            if (_inactive.Count > 0) @event = _inactive.Pop();
            else @event = new CallResultEvent<T>();

            @event._listener = listener;
            SteamworksHelper._activeCallResultEvents.Add(@event);

            @event._callResult.Set(handle, @event.Callback);

            return true;
        }

    } // class CallResultEvent<T>


    public struct SteamworksHelper
    {
        // reference all active CallResultEvent, this prevent them from being garbage collected.
        internal static HashSet<object> _activeCallResultEvents = new HashSet<object>();

        /// <summary>
        /// Init Steamworks. Call it as early as possible.
        /// At development, make sure there is a steam_appid.txt file in your project root directory and the Steam client is running.
        /// </summary>
        /// <param name="appId"></param>
        /// <returns> True if allow running, otherwise false. If it return false, you should quit the game as soon as possible. </returns>
        public static bool Init(uint appId)
        {
            if (SteamAPI.RestartAppIfNecessary(new AppId_t(appId)))
            {
                return false;
            }

            if (!SteamAPI.Init())
            {
                return false;
            }

            RuntimeUtilities.waitForEndOfFrame += SteamAPI.RunCallbacks;

            SteamClient.SetWarningMessageHook(DebugMessage);

            return true;
        }


        [AOT.MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
        static void DebugMessage(int nSeverity, System.Text.StringBuilder pchDebugText)
        {
            if (nSeverity == 0) UnityEngine.Debug.Log(pchDebugText);
            else UnityEngine.Debug.LogWarning(pchDebugText);
        }


        /// <summary>
        /// Quit Steamworks. Call it when the game is quiting.
        /// </summary>
        public static void Shutdown()
        {
            RuntimeUtilities.waitForEndOfFrame -= SteamAPI.RunCallbacks;
            SteamAPI.Shutdown();
        }

    } // struct SteamworksHelper

} // namespace UnityExtensions.Steamworks

#endif // !DISABLESTEAMWORKS