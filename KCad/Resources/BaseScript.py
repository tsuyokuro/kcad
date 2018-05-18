import math

#[AC] putMsg(s)
def putMsg(s):
    SE.PutMsg(s)

#[AC] rect(10, 10, "xy")
def rect(w, h, p):
    SE.Rect(w, h, p)

#[AC] area()
def area():
    return SE.Area()

#[AC] find(range)
def find(range):
    SE.Find(range)

#[AC] findFigId(currentFig())
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

#[AC] addPoint()
def addPoint():
    SE.AddPoint()

#[AC] addLayer(name)
def addLayer(name):
    SE.AddLayer(name)


#[AC] move(x, y, z)
def move(x, y, z):
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
    return (pt.x, pt.y, pt.z)

#[AC] moveCursor(x, y, z)
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

#[AC] faceTo(munitVZ)
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

#[AC] invertDir()
def invertDir():
    SE.InvertDir()

#[AC] sub(idA, idB)
def sub(a, b):
    SE.AsubB(a, b)

#[AC] dumpMesh(currentFig())
def dumpMesh(id):
    SE.DumpMesh(id)

#[AC] addBox(40,40,20)
def addBox(x, y, z):
    SE.AddBox(x, y, z)

#[AC] addCylinder(16, 10, 40)
def addCylinder(slices, r, len):
    SE.AddCylinder(slices, r, len)

#[AC] addSphere(16, 20) # slices, r
def addSphere(slices, r):
    SE.AddSphere(slices, r)

#[AC] extrude(currentFig(), unitVZ, 20)
def extrude(id, v, d):
    SE.Extrude(id, v, d)

#[AC] currentFig()
def currentFig():
    return SE.GetCurrentFigureID()

def test(v):
    SE.Test(v)


#globals
x=0
y=0
z=0

w=10
h=10

ratio=0.5

range = 4

#[AC] point0
point0 = SE.CreateVector(0,0,0)

#[AC] unitVX
#[AC] unitVY
#[AC] unitVZ
unitVX = SE.CreateVector(1,0,0)
unitVY = SE.CreateVector(0,1,0)
unitVZ = SE.CreateVector(0,0,1)

#[AC] munitVX
#[AC] munitVY
#[AC] munitVZ
munitVX = SE.CreateVector(-1,0,0)
munitVY = SE.CreateVector(0,-1,0)
munitVZ = SE.CreateVector(0,0,-1)

