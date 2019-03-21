#pragma once
#pragma unmanaged

#include <Eigen/Core>

class IglFuncs
{
public:
	IglFuncs();

	static Eigen::MatrixXd ArrayToMatrixXd(double* data, int rows, int cols);
	static void ReadOFF(const char* fname, Eigen::MatrixXd & V, Eigen::MatrixXi & F);


	static void Test();

	static void triangulate(
		const Eigen::MatrixXd & V,
		const Eigen::MatrixXi & E,
		const Eigen::MatrixXd & H,
		const std::string flags,
		Eigen::MatrixXd & V2,
		Eigen::MatrixXi & F2);
	
};

