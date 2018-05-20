using System;

namespace Curve {
	/// <summary>
	/// B�X�v���C�����p�����A�m�b�g�Əd�݂̒l�̕ύX���ł���悤�ɁB
	/// </summary>
	public class NURBSCurve : BSplineCurve {
		/// <summary>
		/// �d�݁B
		/// </summary>
		[SerializeField]
		float[] weights;

		/// <summary>
		/// �_�̐ݒ�B
		/// ���ɕύX���Ȃ���Βl���ێ��B
		/// </summary>
		public override void SetPoints(Vector3[] points) {
			this.points = this.closed ? this.CreatePointsAsClosed(points) : points;

			if (this.knots == null || this.points.Length + this.degree + 1 != this.knots.Length)
				this.ResetKnots();

			if (this.weights == null || this.weights.Length != this.points.Length + (this.closed ? this.degree : 0))
				this.ResetWeights();
		}

		/// <summary>
		/// �d�݂��l�����A�w��̈ʒu�̍��W���擾�B
		/// </summary>
		protected override Vector3 CalcuratePoint(float t) {
			Vector3 linePoint = Vector3.zero;
			float weight = 0f;

			for (int i = 0; i < this.points.Length; ++i) {
				float bs = BSplineCurve.BSplineBasisFunc(i, this.degree, t, this.knots);
				linePoint += bs * this.weights[i] * this.points[i];
				weight += bs * this.weights[i];
			}

			return linePoint / weight;
		}

		/// <summary>
		/// �d�݂̃��Z�b�g�B
		/// </summary>
		void ResetWeights() {
			if (this.points == null)
				return;

			this.weights = new float[this.points.Length + (this.closed ? this.degree : 0)];

			for (int i = 0; i < this.weights.Length; ++i) {
				this.weights[i] = 1f;
			}
		}

		/// <summary>
		/// OnValidate.
		/// </summary>
		protected override void OnValidate() {
			if (this.closed || this.passOnEdge) {
				Debug.LogWarning("�T�|�[�g����Ă��܂���Bknots�y��weights�𒼐ڑ��삷��K�v������܂��B");
				this.closed = false;
				this.passOnEdge = false;
			}

			float prevKnot = int.MinValue;
			foreach (float knot in this.knots) {
				if (knot < prevKnot)
					throw new Exception("�m�b�g�̒l���O��̃m�b�g��菬�����A�������͑傫���Ȃ��Ă��܂��B");
				prevKnot = knot;
			}

			if (this.points != null) {
				if (this.knots.Length != this.points.Length + this.degree + 1)
					this.ResetKnots();

				if (this.weights.Length != this.points.Length)
					this.ResetWeights();
			}

			this.DispatchValueChanged();
		}
	}
}