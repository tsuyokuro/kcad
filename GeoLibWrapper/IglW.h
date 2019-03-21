#pragma once

#pragma managed

#include "cad_data_types.h"

#include <Eigen/Core>

using namespace System;

using namespace CadDataTypes;
using namespace MyCollections;


namespace LibiglWrapper {

	public ref class IglW
	{
	public:
		static void Test1(array<double>^ plist, int rows, int cols);
		static void Test2(VectorList^ vl);
		static void Test();

		static CadMesh^ Triangulate(VectorList ^ vl, String ^ option);
		static Eigen::MatrixXd ToMatrixXd2D(VectorList^ m);
		static Eigen::MatrixXd ToMatrixXd(VectorList^ vl);
		static Eigen::MatrixXd ArrayToMatrixXd(double * data, int rows, int cols);
		static CadMesh ^ ReadOFF(String ^ fname);
		static VectorList ^ ToVectorListWithRowMaijor(Eigen::MatrixXd m);
		static VectorList ^ ToVectorListWithRowMaijor2D(Eigen::MatrixXd m);
		static FlexArray<CadFace^>^ ToFaceListWithRowMaijor(Eigen::MatrixXi m);
		static FlexArray<int>^ ToIntListWithRowMaijor(Eigen::MatrixXi m);
	};
}
