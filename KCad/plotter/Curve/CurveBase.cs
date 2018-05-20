using System;

namespace Curve {
	/// <summary>
	/// �C�ӂ̓_����J�[�u��`�悷�邽�߂̍��W��Ԃ��C���^�[�t�F�[�X�B
	/// </summary>
	abstract public class CurveBase {
		public event EventHandler ValueChanged;

		/// <summary>
		/// ����J�[�u�Ƃ��邩�B
		/// </summary>
		public bool closed;

		/// <summary>
		/// ���C���̕������B
		/// </summary>
		public int dividedCount = 100;

		/// <summary>
		/// �C�ӂ̓_�B
		/// </summary>
		protected Vector3[] points;

		/// <summary>
		/// Onvalidate.
		/// </summary>
		protected virtual void OnValidate() {
			if (this.points != null)
				this.DispatchValueChanged();
		}

		/// <summary>
		/// �l���ς�����C�x���g�̔��s�B
		/// </summary>
		protected void DispatchValueChanged() {
			if (this.ValueChanged != null)
				this.ValueChanged(this, EventArgs.Empty);
		}

		/// <summary>
		/// �_�̐ݒ�B
		/// </summary>
		public virtual void SetPoints(Vector3[] points) {
			this.points = points;
		}

		/// <summary>
		/// ���������_��]�����Ԃ��B
		/// </summary>
		public abstract Vector3[] Evaluate();
	}
}

