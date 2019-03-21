#pragma unmanaged
#define _CRT_SECURE_NO_WARNINGS

#include <igl/readOFF.h>
#include <igl/cotmatrix.h>
#include <igl/triangle/triangulate.h>
#include <Eigen/Dense>
#include <Eigen/Sparse>
#include <Eigen/Core>
#include <Eigen/Geometry>

#include <iostream>

#include "IglFuncs.h"


IglFuncs::IglFuncs()
{
}

Eigen::MatrixXd IglFuncs::ArrayToMatrixXd(double * data, int rows, int cols)
{
	Eigen::MatrixXd m(rows, cols);

	int p = 0;

	for (int r = 0; r < rows; r++)
	{
		for (int c = 0; c < cols; c++)
		{
			m(r, c) = data[p++];
		}
	}

	return m;
}

void IglFuncs::ReadOFF(
	const char* fname,
	Eigen::MatrixXd& V,
	Eigen::MatrixXi& F)
{
	igl::readOFF(fname, V, F);
}

void IglFuncs::Test()
{
}

void IglFuncs::triangulate(
	const Eigen::MatrixXd & V,
	const Eigen::MatrixXi & E,
	const Eigen::MatrixXd & H,
	const std::string flags,
	Eigen::MatrixXd & V2,
	Eigen::MatrixXi & F2)
{
	igl::triangle::triangulate(V, E, H, flags, V2, F2);
}