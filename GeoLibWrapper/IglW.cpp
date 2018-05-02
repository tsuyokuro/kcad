#pragma unmanaged

#include "IglFuncs.h"
#include <iostream>


#pragma managed 

#include "IglW.h"

using namespace System::Runtime::InteropServices;
using namespace CadDataTypes;
using namespace MyCollections;


namespace LibiglWrapper
{
	void IglW::Test1(array<double>^ plist, int rows, int cols)
	{
		pin_ptr<double> p = &plist[0];
		Eigen::MatrixXd m = IglFuncs::ArrayToMatrixXd(p, rows, cols);


		std::cout << m;

	}

	void IglW::Test()
	{
		IglFuncs::Test();
	}

	CadMesh^ IglW::ReadOFF(String^ fname)
	{
		char* pfname = (char*)Marshal::StringToHGlobalAnsi(fname).ToPointer();

		Eigen::MatrixXd V;
		Eigen::MatrixXi F;

		IglFuncs::ReadOFF(pfname, V, F);

		CadMesh^ ret = gcnew CadMesh();

		ret->VertexStore = IglW::ToVectorListWithRowMaijor(V);
		ret->FaceStore = IglW::ToFaceListWithRowMaijor(F);

		return ret;
	}

	// RowMaijor
	// 
	VectorList^ IglW::ToVectorListWithRowMaijor(Eigen::MatrixXd m)
	{
		int rows = m.rows();
		int cols = m.cols();

		VectorList^ data = gcnew VectorList(cols * rows);

		CadVector v;

		for (int r = 0; r < m.rows(); r++)
		{
			v.x = m(r, 0);
			v.y = m(r, 1);
			v.z = m(r, 2);

			data->Add(v);
		}
		return data;
	}

	FlexArray<CadFace^>^ IglW::ToFaceListWithRowMaijor(Eigen::MatrixXi m)
	{
		int rows = m.rows();
		int cols = m.cols();

		FlexArray<CadFace^>^ data = gcnew FlexArray<CadFace^>();

		CadFace^ f;

		for (int r = 0; r < rows; r++)
		{
			f = gcnew CadFace();

			for (int c = 0; c < cols; c++)
			{
				f->VList->Add(m(r, c));
			}

			data->Add(f);
		}
		return data;
	}

	FlexArray<int>^ IglW::ToIntListWithRowMaijor(Eigen::MatrixXi m)
	{
		int rows = m.rows();
		int cols = m.cols();

		FlexArray<int>^ data = gcnew FlexArray<int>(cols * rows);

		for (int r = 0; r < m.rows(); r++)
		{
			for (int c = 0; c < cols; c++)
			{
				data->Add(m(r, c));
			}
		}
		return data;
	}
}
