#pragma once

#pragma managed

using namespace System;

using namespace CadDataTypes;
using namespace MyCollections;


namespace LibiglWrapper {

	public ref class IglW
	{
	public:
		static void Test1(array<double>^ plist, int rows, int cols);
		static void Test();


		static CadMesh ^ ReadOFF(String ^ fname);
		static VectorList ^ ToVectorListWithRowMaijor(Eigen::MatrixXd m);
		static FlexArray<CadFace^>^ ToFaceListWithRowMaijor(Eigen::MatrixXi m);
		static FlexArray<int>^ ToIntListWithRowMaijor(Eigen::MatrixXi m);
	};
}
