import time
import math
import clr
clr.AddReference('CadDataTypes')

import CadDataTypes.CadVector as CadVector
import CadDataTypes.VectorList as VectorList

#[AC] puts(s)
def puts(s):
    SE.PutMsg(s)

#[AC] rect(w=10, h=10, p="xy")
def rect(w=10, h=10, p="xy"):
    SE.Rect(w, h, p)
    SE.UpdateTV();

#[AC] rectAt(pv=getldp(), w=10, h=10, p="xy")
def rectAt(pv, w=10, h=10, p="xy"):
    SE.RectAt(pv, w, h, p)
    SE.UpdateTV();

#[AC] area()
def area():
    return SE.Area()

#[AC] find(range)
def find(range):
    SE.Find(range)

#[AC] findFigId(id=currentFigID())
def findFigId(id):
    SE.FindFigureById(id)

#[AC] layerList()
def layerList():
    SE.LayerList()

#[AC] lastDown()
def lastDown():
    return SE.GetLastDownPoint()

#[AC] showVector(v)
def showVector(v):
    SE.ShowVector(v)

#[AC] distance()
def distance():
    SE.Distance()

#[AC] group()
def group():
    SE.Group()

#[AC] ungroup()
def ungroup():
    SE.Ungroup()

#[AC] addPoint(x=0, y=0, z=0)
def addPoint(x, y, z):
    SE.AddPoint(x, y, z)

#[AC] addLayer(name)
def addLayer(name):
    SE.AddLayer(name)


#[AC] move(x=0, y=0, z=0)
def move(x=0, y=0, z=0):
    SE.Move(x, y, z)

#[AC] segLen(len)
def segLen(len):
    SE.SegLen(len)

#[AC] insPoint()
def insPoint():
    SE.InsPoint()

#[AC] createVector(x, y, z)
def createVector(x, y, z):
    return SE.CreateVector(x, y, z)

#[AC] getldp()
def getldp():
    pt = SE.GetLastDownPoint()
    return pt

#[AC] moveCursor(x=10, y=0, z=0)
def moveCursor(x, y, z):
    SE.MoveCursor(x, y, z)

#[AC] setCursor(x, y, z)
def setCursor(x, y, z):
    SE.SetCursor(x, y, z)

#[AC] line(x, y, z)
def line(x, y, z):
    SE.Line(x, y, z)

#[AC] selFig(id)
def selFig(id):
    SE.SelectFigure(id)

#[AC] execScript(fname)
def execScript(fname):
    return SE.ExecPartial(fname)

#[AC] scale(ratio)
def scale(ratio):
    SE.Scale(ratio)

#[AC] rotate(lastDown(), unitVZ, 45)
def rotate(p0, v, t):
    SE.Rotate(p0, v, t)

#[AC] cursorAngleX(d)
def cursorAngleX(d):
    SE.CursorAngleX(d)

#[AC] cursorAngleY(d)
def cursorAngleY(d):
    SE.CursorAngleY(d)

#[AC] toBmp(32, 32)
#[AC] toBmp(32, 32, 0xffffffff, 1, "")
def toBmp(bw, bh, argb=0xffffffff, linew=1, fname=""):
    SE.CreateBitmap(bw, bh, argb, linew, fname)

#[AC] faceTo(dir=unitVZ)
def faceTo(dir):
    SE.FaceToDirection(dir)

#[AC] swapXZ(ax, az)
def swapXZ(ax, az):
    SE.SwapXZ(ax, az)

#[AC] swapYZ(ay, az)
def swapYZ(ay, az):
    SE.SwapYZ(ay, az)

#[AC] projDir()
def projDir():
    return SE.GetProjectionDir()

#[AC] printVector(v)
def printVector(v):
    SE.PrintVector(v)

#[AC] toMesh()
def toMesh():
    SE.ToMesh()
    SE.UpdateTV()


#[AC] invertDir()
def invertDir():
    SE.InvertDir()

#[AC] sub(l_id=1, r_id=2)
def sub(l_id, r_id):
    SE.AsubB(l_id, r_id)
    SE.UpdateTV();


#[AC] union(id1=1, id2=2)
def union(id1, id2):
    SE.Union(id1, id2)
    SE.UpdateTV();

#[AC] intersection(id1=1, id2=2)
def intersection(id1, id2):
    SE.Intersection(id1, id2)
    SE.UpdateTV();

#[AC] dumpMesh(id=currentFigID())
def dumpMesh(id):
    SE.DumpMesh(id)

#[AC] addBox(x=40,y=40,z=20)
def addBox(x, y, z):
    SE.AddBox(x, y, z)
    SE.UpdateTV();

#[AC] spf(x=w_1x4, y=40, z=t_1x4)
def spf(x, y, z):
    SE.AddBox(x, y, z)
    SE.UpdateTV();

#[AC] addCylinder(slices=16, r=10, len=40)
def addCylinder(slices, r, len):
    SE.AddCylinder(slices, r, len)
    SE.UpdateTV();


#[AC] addSphere(slices=16, r=20)
def addSphere(slices, r):
    SE.AddSphere(slices, r)
    SE.UpdateTV();

#âüÇµèoÇµ
#[AC] extrude(id=currentFigID(), dir=unitVZ, d=20, div=0)
def extrude(id, dir, d, div):
    SE.Extrude(id, dir, d, div)
    SE.UpdateTV();

#[AC] currentFigID()
def currentFigID():
    return SE.GetCurrentFigureID()


#[AC] addMoveGide(dir=unitVX)
def addMoveGide(dir):
    SE.AddMoveGide(dir)
    SE.EnableMoveGide(True)

#[AC] resetMoveGide()
def clearMoveGide():
    SE.ClearMoveGide()
    SE.EnableMoveGide(False);

#[AC] enableMoveGide()
def enableMoveGide():
    SE.EnableMoveGide(True)

#[AC] disableMoveGide()
def disableMoveGide():
    SE.EnableMoveGide(False)


#[AC] rotatev(v=unitVX, axis=unitVZ, deg=45.0)
def rotatev(v, axis, deg):
    return SE.RotateVector(v, axis, deg)

#[AC] dumpv(v=unitVX)
def dumpv(v):
    return SE.DumpVector(v)

#[AC] inputPoint()
def inputPoint():
    return SE.InputPoint()

#[AC] inputUnitV()
def inputUnitV():
    return SE.InputUnitVector()


#[AC] test()
def test():
    SE.Test()


#[AC] point0
point0 = SE.CreateVector(0,0,0)

#[AC] unitVX
#[AC] unitVY
#[AC] unitVZ
unitVX = SE.CreateVector(1,0,0)
unitVY = SE.CreateVector(0,1,0)
unitVZ = SE.CreateVector(0,0,1)

w_1x4 = 8.9
t_1x4 = 1.9

EscReset = "\x1b[0m"

EscBalck = "\x1b[30m"
EscRed = "\x1b[31m"
EscGreen = "\x1b[32m"
EscYellow = "\x1b[33m"
EscBlue = "\x1b[34m"
EscMagenta = "\x1b[35m"
EscCyan = "\x1b[36m"
EscWhite = "\x1b[37m"

EscBBalck = "\x1b[90m"
EscBRed = "\x1b[91m"
EscBGreen = "\x1b[92m"
EscBYellow = "\x1b[93m"
EscBBlue = "\x1b[94m"
EscBMagenta = "\x1b[95m"
EscBCyan = "\x1b[96m"
EscBWhite = "\x1b[97m"



#test !!
vv = CadVector.Create(1,1,1);
vl = VectorList();
vl.Add(vv);

