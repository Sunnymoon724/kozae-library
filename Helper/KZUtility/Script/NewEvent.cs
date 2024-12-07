using System;

namespace KZLib
{
	#region NewAction
	public class NewAction
	{
		private Action? _onAction = null;

		public int Count => _onAction?.GetInvocationList().Length ?? 0;

		public NewAction() { }

		public NewAction(Action onAction)
		{
			_onAction = onAction;
		}

		public void AddListener(Action onAction,bool overlap = false)
		{
			IsValid(onAction);

			if(!overlap)
			{
				RemoveListener(onAction);
			}

			_onAction += onAction;
		}

		public void SetListener(Action onAction)
		{
			IsValid(onAction);

			RemoveAllListeners();

			AddListener(onAction,false);
		}

		public void AddListenerAtOnce(Action onAction)
		{
			IsValid(onAction);

			void OnAction()
			{
				onAction.Invoke();

				RemoveListener(OnAction);
			}

			AddListener(OnAction);
		}

		public void RemoveListener(Action onAction)
		{
			IsValid(onAction);

			_onAction -= onAction;
		}

		public void RemoveAllListeners()
		{
			_onAction = null;
		}

		public void Invoke()
		{
			_onAction?.Invoke();
		}

		private void IsValid(Action onAction)
		{
			if(onAction == null)
			{
				throw new ArgumentException("Callback is null");
			}
		}
	}
	#endregion NewAction

	// -----------------------------------------------------------------------------------------------

	#region NewAction<TObject>
	public class NewAction<TObject>
	{
		private Action<TObject>? _onAction = null;

		public int Count => _onAction?.GetInvocationList().Length ?? 0;

		public NewAction() { }

		public NewAction(Action<TObject> onAction)
		{
			_onAction = onAction;
		}

		public void AddListener(Action<TObject> onAction,bool overlap = false)
		{
			IsValid(onAction);

			if(!overlap)
			{
				RemoveListener(onAction);
			}

			_onAction += onAction;
		}

		public void SetListener(Action<TObject> onAction)
		{
			IsValid(onAction);

			RemoveAllListeners();

			AddListener(onAction,false);
		}

		public void AddListenerAtOnce(Action<TObject> onAction)
		{
			IsValid(onAction);

			void OnAction(TObject objectT)
			{
				onAction.Invoke(objectT);

				RemoveListener(OnAction);
			}

			AddListener(OnAction);
		}

		public void RemoveListener(Action<TObject> onAction)
		{
			IsValid(onAction);

			_onAction -= onAction;
		}

		public void RemoveAllListeners()
		{
			_onAction = null;
		}

		public void Invoke(TObject _objectT)
		{
			_onAction?.Invoke(_objectT);
		}

		private void IsValid(Action<TObject> onAction)
		{
			if(onAction == null)
			{
				throw new ArgumentException("Callback is null");
			}
		}
	}
	#endregion NewAction<TObject>

	// -----------------------------------------------------------------------------------------------

	#region NewFunc<TResult>
	public class NewFunc<TResult>
	{
		private Func<TResult>? _onFunc = null;

		public int Count => _onFunc?.GetInvocationList().Length ?? 0;

		public NewFunc() { }

		public NewFunc(Func<TResult> onFunc)
		{
			_onFunc = onFunc;
		}

		public void AddListener(Func<TResult> onFunc,bool overlap = false)
		{
			IsValid(onFunc);

			if(!overlap)
			{
				RemoveListener(onFunc);
			}

			_onFunc += onFunc;
		}

		public void SetListener(Func<TResult> onFunc)
		{
			IsValid(onFunc);

			RemoveAllListeners();

			AddListener(onFunc,false);
		}

		public void AddListenerAtOnce(Func<TResult> onFunc)
		{
			IsValid(onFunc);

			TResult OnFunc()
			{
				RemoveListener(OnFunc);

				return onFunc.Invoke();
			}

			AddListener(OnFunc);
		}

		public void RemoveListener(Func<TResult> onFunc)
		{
			IsValid(onFunc);

			_onFunc -= onFunc;
		}

		public void RemoveAllListeners()
		{
			_onFunc = null;
		}

		public TResult Invoke()
		{
			return _onFunc != null ? _onFunc.Invoke() : throw new NullReferenceException("Callback is null");
		}

		private void IsValid(Func<TResult> onFunc)
		{
			if(onFunc == null)
			{
				throw new ArgumentException("Callback is null");
			}
		}
	}
	#endregion NewFunc<TResult>

	// -----------------------------------------------------------------------------------------------

	#region NewFunc<TObject,TResult>
	public class NewFunc<TObject,TResult>
	{
		private Func<TObject,TResult>? _onFunc = null;

		public int Count => _onFunc?.GetInvocationList().Length ?? 0;

		public NewFunc() { }

		public NewFunc(Func<TObject,TResult> onFunc)
		{
			_onFunc = onFunc;
		}

		public void AddListener(Func<TObject,TResult> onFunc,bool overlap = false)
		{
			IsValid(onFunc);

			if(!overlap)
			{
				RemoveListener(onFunc);
			}

			_onFunc += onFunc;
		}

		public void SetListener(Func<TObject,TResult> onFunc)
		{
			IsValid(onFunc);

			RemoveAllListeners();

			AddListener(onFunc,false);
		}

		public void AddListenerAtOnce(Func<TObject,TResult> onFunc)
		{
			IsValid(onFunc);

			TResult OnFunc(TObject objectT)
			{
				RemoveListener(OnFunc);

				return onFunc.Invoke(objectT);
			}

			AddListener(OnFunc);
		}

		public void RemoveListener(Func<TObject,TResult> onFunc)
		{
			IsValid(onFunc);

			_onFunc -= onFunc;
		}

		public void RemoveAllListeners()
		{
			_onFunc = null;
		}

		public TResult Invoke(TObject _objectT)
		{
			return _onFunc != null ? _onFunc.Invoke(_objectT) : throw new NullReferenceException("Callback is null");
		}

		private void IsValid(Func<TObject,TResult> onFunc)
		{
			if(onFunc == null)
			{
				throw new ArgumentException("Callback is null");
			}
		}
	}
	#endregion NewFunc<TObject,TResult>
}