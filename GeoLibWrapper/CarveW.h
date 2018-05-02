#pragma once

using namespace CadDataTypes;

namespace CarveW
{
	ref class CarveW
	{
	public:
		CarveW();
		CadMesh ^ CrateCylinder(int slices, double rad, double height);
	};
}


