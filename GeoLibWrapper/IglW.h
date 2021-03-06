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
		static void Test2(VertexList^ vl);
		static void Test();

		static CadMesh^ Triangulate(VertexList^ vl, String ^ option);
		static Eigen::MatrixXd ToMatrixXd2D(VertexList^ m);
		static Eigen::MatrixXd ToMatrixXd(VertexList^ vl);
		static Eigen::MatrixXd ArrayToMatrixXd(double * data, int rows, int cols);
		static CadMesh ^ ReadOFF(String ^ fname);
		static VertexList^ ToVectorListWithRowMaijor(Eigen::MatrixXd m);
		static VertexList^ ToVectorListWithRowMaijor2D(Eigen::MatrixXd m);
		static FlexArray<CadFace^>^ ToFaceListWithRowMaijor(Eigen::MatrixXi m);
		static FlexArray<int>^ ToIntListWithRowMaijor(Eigen::MatrixXi m);
	};
}
