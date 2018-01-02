using System;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
	[AddComponentMenu ("Layout/Content Size Fitter", 141), ExecuteInEditMode, RequireComponent (typeof(RectTransform))]
	public class LazyContentSizeFitter : UIBehaviour, ILayoutSelfController, ILayoutController
	{
		//
		// Fields
		//
		[SerializeField]
		protected ContentSizeFitter.FitMode m_HorizontalFit = ContentSizeFitter.FitMode.Unconstrained;

		[SerializeField]
		protected ContentSizeFitter.FitMode m_VerticalFit = ContentSizeFitter.FitMode.Unconstrained;

		[NonSerialized]
		private RectTransform m_Rect;

		private DrivenRectTransformTracker m_Tracker;

		//
		// Properties
		//
		public ContentSizeFitter.FitMode horizontalFit {
			get {
				return this.m_HorizontalFit;
			}
			set {
				if (this.m_HorizontalFit != value) {
					this.m_HorizontalFit = value;
					this.SetDirty ();
				}
			}
		}

		private RectTransform rectTransform {
			get {
				if (this.m_Rect == null) {
					this.m_Rect = base.GetComponent<RectTransform> ();
				}
				return this.m_Rect;
			}
		}

		public ContentSizeFitter.FitMode verticalFit {
			get {
				return this.m_VerticalFit;
			}
			set {
				if (this.m_VerticalFit != value) {
					this.m_VerticalFit = value;
					this.SetDirty ();
				}
			}
		}

		//
		// Constructors
		//
		protected LazyContentSizeFitter ()
		{
		}

		//
		// Methods
		//
		private void HandleSelfFittingAlongAxis (int axis)
		{
			ContentSizeFitter.FitMode fitMode = (axis != 0) ? this.verticalFit : this.horizontalFit;
			if (fitMode == ContentSizeFitter.FitMode.Unconstrained) {
				this.m_Tracker.Add (this, this.rectTransform, 0);
			}
			else {
				this.m_Tracker.Add (this, this.rectTransform, (axis != 0) ? DrivenTransformProperties.SizeDeltaY : DrivenTransformProperties.SizeDeltaX);
				if (fitMode == ContentSizeFitter.FitMode.MinSize) {
					this.rectTransform.SetSizeWithCurrentAnchors ((RectTransform.Axis)axis, LayoutUtility.GetMinSize (this.m_Rect, axis));
				}
				else {
					this.rectTransform.SetSizeWithCurrentAnchors ((RectTransform.Axis)axis, LayoutUtility.GetPreferredSize (this.m_Rect, axis));
				}
			}
		}

		protected override void OnDisable ()
		{
			this.m_Tracker.Clear (true);
			LayoutRebuilder.MarkLayoutForRebuild (this.rectTransform);
			base.OnDisable ();
		}

		protected override void OnEnable ()
		{
			base.OnEnable ();
			this.SetDirty ();
		}

		Vector3[] m_prevDimentionChangeWorldCorners = new Vector3[4];
		Vector3[] m_tempDimentionChangeWorldCorners = new Vector3[4];
		protected override void OnRectTransformDimensionsChange ()
		{
			bool changed = false;
			rectTransform.GetWorldCorners (m_tempDimentionChangeWorldCorners);
			for (int i = 0; i < 4; i++) {
				Debug.Log (i + " " + m_prevDimentionChangeWorldCorners [i] + " " + m_tempDimentionChangeWorldCorners [i]);
				if (m_prevDimentionChangeWorldCorners [i] != m_tempDimentionChangeWorldCorners[i]) {
					changed = true;
					m_prevDimentionChangeWorldCorners [i] = m_tempDimentionChangeWorldCorners [i];
				}
			}

			if (changed) {
				Debug.Log ("KEKE");
				this.SetDirty ();
			} else {
				Debug.Log ("Lolo");
			}
		}

		protected override void OnValidate ()
		{
			this.SetDirty ();
		}

		protected void SetDirty ()
		{
			if (this.IsActive ()) {
				Debug.Log ("Shet");
				LayoutRebuilder.MarkLayoutForRebuild (this.rectTransform);
			}
		}

		public virtual void SetLayoutHorizontal ()
		{
			this.m_Tracker.Clear (true);
			this.HandleSelfFittingAlongAxis (0);
		}

		public virtual void SetLayoutVertical ()
		{
			this.HandleSelfFittingAlongAxis (1);
		}

		//
		// Nested Types
		//
		public enum FitMode
		{
			Unconstrained,
			MinSize,
			PreferredSize
		}
	}
}
