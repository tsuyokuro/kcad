# coding: cp932
from datetime import datetime as dt
import time
import math
import sys
from System.Collections import *
from System.Collections.Generic import List
from System import UInt32 as uint

import clr
clr.AddReference('CadDataTypes')

import CadDataTypes.CadVector as CadVector
import CadDataTypes.VectorList as VectorList

#[AC] puts(s)
def puts(s):
    SE.PutMsg(s)

#[AC] add_rect(w=10, h=10)
def add_rect(w=10, h=10):
    SE.Rect(w, h)

#[AC] add_rect_at(pv=lastDown(), w=10, h=10)
def add_rect_at(pv, w=10, h=10):
    SE.RectAt(pv, w, h)

#[AC] area()
def area():
    return SE.Area()

#[AC] find_fig_id(id=current_fig_id())
def find_fig_id(id):
    SE.FindFigureById(id)

#[AC] layer_list()
def layer_list():
    SE.LayerList()

#[AC] last_down()
def last_down():
    return SE.GetLastDownPoint()

#[AC] get_selected_fig_list()
def get_selected_fig_list():
	return SE.GetSelectedFigList()

#[AC] to_fig_list(id_list=[1,2])
def to_fig_list(id_list):
	return SE.ToFigList(id_list)

#[AC] to_fig_id_array(list)
def to_fig_id_array(list):
	ret = []
	for i in range(list.Count):
		f = list[i]
		ret = ret + [int(f.ID)]
	return ret

#[AC] group(list=get_selected_fig_list())
#[AC] group(list=[1,2])
def group(list):
    SE.Group(list)

#[AC] ungroup(list=get_selected_fig_list())
#[AC] ungroup(list=[1,2])
#[AC] ungroup(1)
def ungroup(list):
    SE.Ungroup(list)

#[AC] add_point(x=0, y=0, z=0)
def add_point(x, y, z):
    SE.AddPoint(x, y, z)

#[AC] add_point_v(last_down())
def add_point_v(p):
    SE.AddPoint(p)

#[AC] add_layer(name)
def add_layer(name):
    SE.AddLayer(name)


#[AC] move(id=current_fig_id(), x=0, y=0, z=0)
def move(id, x=0, y=0, z=0):
    SE.Move(id, x, y, z)

#[AC] move_selected_point(x=0, y=0, z=0)
def move_selected_point(x=0, y=0, z=0):
    SE.MoveSelectedPoint(x, y, z)

#[AC] ins_point()
def ins_point():
    SE.InsPoint()

#[AC] create_vector(x, y, z)
def create_vector(x, y, z):
    return SE.CreateVector(x, y, z)

#[AC] get_last_down()
def get_last_down():
    pt = SE.GetLastDownPoint()
    return pt

#[AC] move_last_down(x=10, y=0, z=0)
def move_last_down(x, y, z):
    SE.MoveLastDownPoint(x, y, z)

#[AC] set_last_down(x=0, y=0, z=0)
def set_last_down(x, y, z):
    SE.SetLastDownPoint(x, y, z)

#[AC] sel_fig(id)
def sel_fig(id):
    SE.SelectFigure(id)

#[AC] scale(id=current_fig_id(), org=lastDown(), ratio=1.5)
def scale(id, org, ratio):
    SE.Scale(id, org, ratio)

#[AC] rotate(current_fig_id(), inputPoint(), viewDir(), 45)
def rotate(id, p0, v, t):
    SE.Rotate(id, p0, v, t)


#[AC] proj_dir()
def proj_dir():
    return SE.GetProjectionDir()

#[AC] print_vector(v)
def print_vector(v):
    SE.PrintVector(v)

#[AC] to_mesh(current_fig_id())
def to_mesh(id):
    SE.ToMesh(id)

#[AC] to_poly(current_fig_id())
def to_poly(id):
    SE.ToPolyLine(id)

#[AC] invert_dir()
def invert_dir():
    SE.InvertDir()

#[AC] sub(l_id=1, r_id=2)
def sub(l_id, r_id):
    SE.AsubB(l_id, r_id)


#[AC] union(id1=1, id2=2)
def union(id1, id2):
    SE.Union(id1, id2)

#[AC] intersection(id1=1, id2=2)
def intersection(id1, id2):
    SE.Intersection(id1, id2)

#[AC] dump_mesh(id=current_fig_id())
def dump_mesh(id):
    SE.DumpMesh(id)

#[AC] add_box(x=40,y=40,z=20)
def add_box(x, y, z):
    SE.AddBox(x, y, z)

#[AC] spf(x=w_1x4, y=40, z=t_1x4)
def spf(x, y, z):
    SE.AddBox(x, y, z)

#[AC] add_cylinder(slices=16, r=10, len=40)
def add_cylinder(slices, r, len):
    SE.AddCylinder(slices, r, len)


#[AC] add_sphere(slices=16, r=20)
def add_sphere(slices, r):
    SE.AddSphere(slices, r)

#[AC] extrude(id=current_fig_id(), dir=unitVZ, d=20, div=0)
def extrude(id, dir, d, div):
    SE.Extrude(id, dir, d, div)

#[AC] current_fig_id()
def current_fig_id():
    return SE.GetCurrentFigureID()

#[AC] currentFig()
def currentFig():
    return SE.GetCurrentFigure()

#[AC] rotatev(v=unitVX, axis=unitVZ, deg=45.0)
def rotatev(v, axis, deg):
    return SE.RotateVector(v, axis, deg)

#[AC] dumpv(v=unitVX)
def dumpv(v):
    return SE.DumpVector(v)

#[AC] input_point()
def input_point():
    return SE.InputPoint()

#[AC] input_unit_v()
def input_unit_v():
    return SE.InputUnitVector()

#[AC] update_tree()
def update_tree():
    SE.UpdateTV()

#[AC] view_dir()
def view_dir():
	return SE.ViewDir()

#[AC] triangulate(id=current_fig_id(), area=10000, deg=20)
def triangulate(id, area, deg):
    SE.Triangulate(id, area, deg)

#[AC] triangulate_opt(id=current_fig_id(), option="a10000q")
def triangulate_opt(id, option):
    SE.Triangulate(id, option)

#[AC] add_line(create_vector(0, 0, 0), create_vector(10, 20, 0))
def add_line(v0, v1):
    SE.AddLine(v0, v1)


# Need to consider >>>>

#[AC] set_seg_len(len)
def set_seg_len(len):
    SE.SetSelectedSegLen(len)

#[AC] to_bmp(32, 32)
#[AC] to_bmp(32, 32, 0xffffffff, 1, "")
def to_bmp(bw, bh, argb=0xffffffff, linew=1, fname=""):
    SE.CreateBitmap(bw, bh, argb, linew, fname)

#[AC] faceTo(dir=unitVZ)
def face_to(dir):
    SE.FaceToDirection(dir)

#[AC] devPToWorldP(p)
def devPToWorldP(p):
    return SE.DevPToWorldP(p)

#[AC] worldPToDevP(p)
def worldPToDevP(p):
    return SE.WorldPToDevP(p)

#[AC] devVToWorldV(v)
def devPToWorldP(v):
    return SE.DevVToWorldV(v)

#[AC] worldVToDevV(v)
def worldVToDevV(v):
    return SE.WorldVToDevV(v)

# <<<< Need to consider

#[AC] test()
def test():
	SE.Test()


class MyConsoleOut:
	def write(self, s):
		SE.PutMsg(s)

cout = MyConsoleOut()

sys.stdout = cout

#[AC] point0
point0 = SE.CreateVector(0,0,0)

#[AC] unit_vx
#[AC] unit_vy
#[AC] unit_vz
unit_vx = SE.CreateVector(1,0,0)
unit_vy = SE.CreateVector(0,1,0)
unit_vz = SE.CreateVector(0,0,1)

w_1x4 = 89
t_1x4 = 19


# Text color escape sequence
#[AC] esc_balck
#[AC] esc_red
#[AC] esc_green
#[AC] esc_yellow 
#[AC] esc_blue 
#[AC] esc_magenta
#[AC] esc_cyan 
#[AC] esc_white

#[AC] esc_b_balck
#[AC] esc_b_red
#[AC] esc_b_green
#[AC] esc_b_yellow 
#[AC] esc_b_blue 
#[AC] esc_b_magenta
#[AC] esc_b_cyan
#[AC] esc_b_white

esc_reset = "\x1b[0m"

# Normal color
esc_balck = "\x1b[30m"
esc_red = "\x1b[31m"
esc_green = "\x1b[32m"
esc_yellow = "\x1b[33m"
esc_blue = "\x1b[34m"
esc_magenta = "\x1b[35m"
esc_cyan = "\x1b[36m"
esc_white = "\x1b[37m"

# Bright color
esc_b_balck = "\x1b[90m"
esc_b_red = "\x1b[91m"
esc_b_green = "\x1b[92m"
esc_b_yellow = "\x1b[93m"
esc_b_blue = "\x1b[94m"
esc_b_magenta = "\x1b[95m"
esc_b_cyan = "\x1b[96m"
esc_b_white = "\x1b[97m"


#test !!
vv = CadVector.Create(1,1,1);
vl = VectorList();
vl.Add(vv);

