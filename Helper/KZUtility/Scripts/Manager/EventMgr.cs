using System;
using System.Collections.Generic;

namespace KZLib.KZUtility
{
	public class EventMgr : Singleton<EventMgr>
	{
		private readonly Dictionary<string,Delegate> m_listenerDict = new Dictionary<string,Delegate>();

		private bool m_disposed = false;

		protected override void Release(bool disposing)
		{
			if(m_disposed)
			{
				return;
			}

			if(disposing)
			{
				m_listenerDict.Clear();
			}

			m_disposed = true;

			base.Release(disposing);
		}

		public void EnableListener(string eventName,Action onAction)
		{
			_EnableListener(eventName,onAction);
		}

		public void EnableListener<TParam>(string eventName,Action<TParam> onAction)
		{
			_EnableListener(eventName,onAction);
		}

		private void _EnableListener(string eventName,Delegate callback)
		{
			_ValidateCallback(callback);

			if(m_listenerDict.TryGetValue(eventName,out var listener) && listener != null)
			{
				_ValidateType(eventName,callback,listener);

				m_listenerDict[eventName] = Delegate.Combine(listener,callback);
			}
			else
			{
				m_listenerDict[eventName] = callback;
			}
		}

		public void DisableListener(string eventName,Action onAction)
		{
			_DisableListener(eventName,onAction);
		}

		public void DisableListener<TDelegate>(string eventName,Action<TDelegate> onAction)
		{
			_DisableListener(eventName,onAction);
		}

		private void _DisableListener(string eventName,Delegate callback)
		{
			_ValidateCallback(callback);

			if(!m_listenerDict.TryGetValue(eventName,out var listener) || listener == null)
			{
				return;
			}

			_ValidateType(eventName,callback,listener);

			m_listenerDict[eventName] = Delegate.Remove(listener,callback);

			if(m_listenerDict[eventName] == null)
			{
				m_listenerDict.Remove(eventName);
			}
		}

		public void SendEvent(string eventName)
		{
			if(m_listenerDict.TryGetValue(eventName,out var listener) && listener is Action onAction)
			{
				onAction();
			}
			else
			{
				throw new ArgumentNullException($"{listener.GetType().Name} is not in {eventName}.");
			}
		}

		public void SendEvent<TParam>(string eventName,TParam param)
		{
			if(m_listenerDict.TryGetValue(eventName,out var listener) && listener is Action<TParam> onAction)
			{
				onAction(param);
			}
			else
			{
				throw new ArgumentNullException($"{listener.GetType().Name} is not in {eventName}.");
			}
		}

		private void _ValidateCallback(Delegate callback)
		{
			if(callback == null)
			{
				throw new NullReferenceException("Callback is null");
			}
		}

		private void _ValidateType(string eventName,Delegate callback,Delegate listener)
		{
			var listenerType = listener.GetType();
			var callbackType = callback.GetType();

			if(listenerType != callbackType)
			{
				throw new InvalidOperationException($"{listenerType.Name} != {callbackType.Name} in {eventName}");
			}
		}
	}
}