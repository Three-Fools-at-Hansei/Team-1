using System;
using System.Threading.Tasks;
using UnityEngine;
//using static UnityEditor.Profiling.HierarchyFrameDataView;

namespace UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UI_View : MonoBehaviour
    {
        /// <summary>
        /// View�� ��ȣ�ۿ��� ViewModel
        /// </summary>
        public IViewModel ViewModel { get; private set; }

        /// <summary>
        /// UI ������ �����ϱ� ���� CanvasGroup
        /// </summary>
        protected CanvasGroup _canvasGroup;

        protected virtual void Awake()
        {
            _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
        }

        /// <summary>
        /// �� View�� ��ȣ�ۿ��� ViewModel�� ����(����)�ϰ� ������ ���ε��� �����մϴ�.
        /// </summary>
        /// <param name="viewModel">������ ViewModel�Դϴ�.</param>
        public virtual void SetViewModel(IViewModel viewModel)
        {
            // ���� ViewModel�� �ִٸ� �̺�Ʈ ������ �����Ͽ� �޸� ������ �����մϴ�.
            if (ViewModel != null)
                ViewModel.OnStateChanged -= OnStateChanged;

            ViewModel = viewModel;

            // ���ο� ViewModel�� ���� ���� �̺�Ʈ�� �����մϴ�.
            if (ViewModel != null)
                ViewModel.OnStateChanged += OnStateChanged;

            // ViewModel�� ������ ����, �ʱ� �����͸� UI�� �ݿ��ϱ� ���� OnStateChanged�� ȣ���մϴ�.
            OnStateChanged();
        }

        /// <summary>
        /// ViewModel�� ���°� ����Ǿ��� �� ȣ��Ǵ� �޼����Դϴ�.
        /// �Ļ� Ŭ������ �� �޼��带 �������Ͽ� UI ������Ʈ�� �����ؾ� �մϴ�.
        /// </summary>
        protected abstract void OnStateChanged();

        /// <summary>
        /// ������Ʈ �ı� �� �̺�Ʈ ������ Ȯ���� �����մϴ�.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (ViewModel != null)
                ViewModel.OnStateChanged -= OnStateChanged;
        }
    }
}