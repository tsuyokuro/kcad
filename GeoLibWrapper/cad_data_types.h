#pragma once
#ifdef WIN32
#ifdef _DEBUG
#using "..\\CadDataTypes\\bin\\x86\\Debug\\CadDataTypes.dll"
#else
#using "..\\CadDataTypes\\bin\\x86\\Release\\CadDataTypes.dll"
#endif
#else
#ifdef _DEBUG
#using "..\\CadDataTypes\\bin\\x64\\Debug\\CadDataTypes.dll"
#else
#using "..\\CadDataTypes\\bin\\x64\\Release\\CadDataTypes.dll"
#endif
#endif
