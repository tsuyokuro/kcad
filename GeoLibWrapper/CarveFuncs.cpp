#include <carve/csg.hpp>
#include <carve/input.hpp>

#include "CarveFuncs.h"
#include "geometry.hpp"


CarveFuncs::CarveFuncs()
{
}


CarveFuncs::~CarveFuncs()
{
}


#define DIM 60


void CarveFuncs::Test()
{
	carve::poly::Polyhedron *a = makeTorus(30, 30, 2.0, 0.8, carve::math::Matrix::ROT(0.5, 1.0, 1.0, 1.0));

	carve::input::PolyhedronData data;

	for (int i = 0; i < DIM; i++) {
		double x = -3.0 + 6.0 * i / double(DIM - 1);
		for (int j = 0; j < DIM; j++) {
			double y = -3.0 + 6.0 * j / double(DIM - 1);
			double z = -1.0 + 2.0 * cos(sqrt(x * x + y * y) * 2.0) / sqrt(1.0 + x * x + y * y);
			size_t n = data.addVertex(carve::geom::VECTOR(x, y, z));
			if (i && j) {
				data.addFace(n - DIM - 1, n - 1, n - DIM);
				data.addFace(n - 1, n, n - DIM);
			}
		}
	}

	for (int i = 0; i < DIM; i++) {
		double x = -3.0 + 6.0 * i / double(DIM - 1);
		for (int j = 0; j < DIM; j++) {
			double y = -3.0 + 6.0 * j / double(DIM - 1);
			double z = 1.0 + 2.0 * cos(sqrt(x * x + y * y) * 2.0) / sqrt(1.0 + x * x + y * y);
			size_t n = data.addVertex(carve::geom::VECTOR(x, y, z));
			if (i && j) {
				data.addFace(n - DIM - 1, n - 1, n - DIM);
				data.addFace(n - 1, n, n - DIM);
			}
		}
	}

	carve::poly::Polyhedron *b = data.create();

	carve::poly::Polyhedron *c = carve::csg::CSG().compute(a, b, carve::csg::CSG::A_MINUS_B);
}