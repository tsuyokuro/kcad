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
clr.AddReference('OpenTK')

import CadDataTypes.CadVertex as CadVertex
import CadDataTypes.VertexList as VertexList
import CadDataTypes.CadMesh as CadMesh
import CadDataTypes.CadFace as CadFace

import OpenTK.Vector3d as Vector3d


###############################################################################
# Layer
#

#[AC] layer_list()
def layer_list():
    SE.LayerList()

#[AC] add_layer(name)
def add_layer(name):
    SE.AddLayer(name)


###############################################################################
# Last down point
#

#[AC] last_down()
def last_down():
    return SE.GetLastDownPoint()

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


###############################################################################
# Figure list
#

#[AC] find_fig_by_id(id=current_fig_id())
def find_fig_by_id(id):
    SE.FindFigureById(id)

#[AC] sel_fig(id)
def sel_fig(id):
    SE.SelectFigure(id)

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

###############################################################################
# Group management
#

#[AC] group(list=get_selected_fig_list())
#[AC] group(list=[1,2])
def group(list):
    SE.Group(list)

#[AC] ungroup(list=get_selected_fig_list())
#[AC] ungroup(list=[1,2])
#[AC] ungroup(1)
def ungroup(list):
    SE.Ungroup(list)


###############################################################################
# Boolean operation
#

#[AC] sub(l_id=1, r_id=2)
def sub(l_id, r_id):
    SE.AsubB(l_id, r_id)

#[AC] union(id1=1, id2=2)
def union(id1, id2):
    SE.Union(id1, id2)

#[AC] intersection(id1=1, id2=2)
def intersection(id1, id2):
    SE.Intersection(id1, id2)



###############################################################################
# Add figure
#

#[AC] add_rect(w=10, h=10)
def add_rect(w=10, h=10):
    return SE.AddRect(w, h)

#[AC] add_rect_at(pv=last_down(), w=10, h=10)
def add_rect_at(pv, w=10, h=10):
    return SE.AddRectAt(pv, w, h)


#[AC] add_rectc(w=10, h=10, c=1)
def add_rectc(w=10, h=10, c=1):
    return SE.AddRectChamfer(w, h, c)

#[AC] add_rectc_at(pv=last_down(), w=10, h=10, c=1)
def add_rectc_at(pv, w=10, h=10, c=1):
    return SE.AddRectChamferAt(pv, w, h, c)


#[AC] add_point(x=0, y=0, z=0)
def add_point(x, y, z):
    return SE.AddPoint(x, y, z)

#[AC] add_point_v(last_down())
def add_point_v(p):
    return SE.AddPoint(p)

#[AC] add_box(last_down(), size_x=40, size_y=40, size_z=20)
def add_box(pos, size_x, size_y, size_z):
    return SE.AddBox(pos, size_x, size_y, size_z)

#[AC] add_1x4(last_down(), len=200)
def add_1x4(pos, len):
    return SE.AddBox(pos, w_1x4, len, t_1x4)

#[AC] add_cylinder(pos=last_down(), slices=16, r=10, len=40)
def add_cylinder(pos, slices, r, len):
    return SE.AddCylinder(pos, slices, r, len)

#[AC] add_sphere(pos=last_down(), slices=16, r=20)
def add_sphere(pos, slices, r):
    return SE.AddSphere(pos, slices, r)

#[AC] add_line(vector(0, 0, 0), vector(10, 20, 0))
def add_line(v0, v1):
    return SE.AddLine(v0, v1)

#[AC] add_tetra(last_down(), size_x=20, size_y=20, size_z=20)
def add_tetra(pos, size_x, size_y, size_z):
    return SE.AddTetrahedron(pos, size_x, size_y, size_z)

#[AC] add_octa(last_down(), size_x=20, size_y=20, size_z=20)
def add_octa(pos, size_x, size_y, size_z):
    return SE.AddOctahedron(pos, size_x, size_y, size_z)


###############################################################################
# Edit

#[AC] extrude(id=current_fig_id(), dir=unit_vz, d=20, div=0)
def extrude(id, dir, d, div):
    SE.Extrude(id, dir, d, div)

#[AC] move(id=current_fig_id(), x=0, y=0, z=0)
def move(id, x=0, y=0, z=0):
    SE.Move(id, x, y, z)

#[AC] move_selected_point(x=0, y=0, z=0)
def move_selected_point(x=0, y=0, z=0):
    SE.MoveSelectedPoint(x, y, z)

#[AC] triangulate(id=current_fig_id(), area=10000, deg=20)
def triangulate(id, area, deg):
    SE.Triangulate(id, area, deg)

#[AC] triangulate_opt(id=current_fig_id(), option="a10000q")
def triangulate_opt(id, option):
    SE.Triangulate(id, option)

#[AC] to_mesh(current_fig_id())
def to_mesh(id):
    SE.ToMesh(id)

#[AC] to_poly(current_fig_id())
def to_poly(id):
    SE.ToPolyLine(id)

#[AC] invert_dir()
def invert_dir():
    SE.InvertDir()

#[AC] scale(id=current_fig_id(), org=last_down(), ratio=1.5)
def scale(id, org, ratio):
    SE.Scale(id, org, ratio)

#[AC] rotate(current_fig_id(), input_point(), view_dir(), 45)
def rotate(id, p0, v, t):
    SE.Rotate(id, p0, v, t)

#[AC] ins_point()
def ins_point():
    SE.InsPoint()

#[AC] get_str(msg="Input", defStr="")
def get_str(msg, defStr):
    return SE.GetString(msg, defStr)

#[AC] set_seg_len(len)
def set_seg_len(len):
    SE.SetSelectedSegLen(len)

#[AC] set_fig_name(id=current_fig_id(), name="name")
def set_fig_name(id, name):
    SE.SetFigName(id, name)

#[AC] get_point(figID=current_fig_id(), index=0)
def get_point(figID, index):
	return SE.GetPoint(figID, index);

#[AC] set_point(figID=current_fig_id(), index=0, dv=Vector3d(0,0,0))
def set_point(figID, index, dv):
	return SE.SetPoint(figID, index, dv);

###############################################################################


#[AC] current_fig_id()
def current_fig_id():
    return SE.GetCurrentFigureID()

#[AC] currentFig()
def currentFig():
    return SE.GetCurrentFigure()


###############################################################################

#[AC] vector(x, y, z)
def vector(x, y, z):
    return SE.CreateVector(x, y, z)

#[AC] vertex(x, y, z)
def vertex(x, y, z):
    return SE.CreateVertex(x, y, z)

#[AC] rotatev(v=unit_vx, axis=unit_vz, deg=45.0)
def rotatev(v, axis, deg):
    return SE.RotateVector(v, axis, deg)

###############################################################################
# Print information
#

#[AC] puts(s)
def puts(s):
    SE.PutMsg(s)

#[AC] print_vector(v)
def print_vector(v):
    SE.PrintVector(v)

#[AC] dumpv(v=unit_vx)
def dumpv(v):
    return SE.DumpVector(v)

#[AC] dump_mesh(id=current_fig_id())
def dump_mesh(id):
    SE.DumpMesh(id)


###############################################################################
# Interraction
#

#[AC] input_point()
def input_point():
    return SE.InputPoint()

#[AC] input_unit_v()
def input_unit_v():
    return SE.InputUnitVector()


###############################################################################
# User interface
#

#[AC] update_tree()
def update_tree():
    SE.UpdateTV()

#[AC] view_dir()
def view_dir():
	return SE.ViewDir()

#[AC] proj_dir()
def proj_dir():
    return SE.GetProjectionDir()

#[AC] cut_mesh(id=current_fig_id())
def cut_mesh(id):
    SE.CutMesh(id)

###############################################################################
# Mesure
#

#[AC] area_of_selected()
def area_of_selected():
    return SE.AreaOfSelected()

#[AC] centroid_of_selected()
def centroid_of_selected():
	return SE.CentroidOfSelected()

###############################################################################

#[AC] to_bmp(32, 32)
#[AC] to_bmp(32, 32, 0xffffffff, 1, "")
def to_bmp(bw, bh, argb=0xffffffff, linew=1, fname=""):
    SE.CreateBitmap(bw, bh, argb, linew, fname)

#[AC] dev_p_to_world_p(p)
def dev_p_to_world_p(p):
    return SE.DevPToWorldP(p)

#[AC] world_p_to_dev_p(p)
def world_p_to_dev_p(p):
    return SE.WorldPToDevP(p)

#[AC] rad2deg(rad)
def rad2deg(rad):
	return 180.0 * rad / math.pi

#[AC] deg2rad(deg)
def deg2rad(deg):
	return math.pi * deg / 180.0


#[AC] test()
def test():
	SE.Test()


class MyConsoleOut:
	def write(self, s):
		SE.Print(s)

cout = MyConsoleOut()

sys.stdout = cout

#[AC] point0
point0 = Vector3d(0,0,0)

#[AC] unit_vx
#[AC] unit_vy
#[AC] unit_vz
unit_vx = Vector3d(1,0,0)
unit_vy = Vector3d(0,1,0)
unit_vz = Vector3d(0,0,1)

#[AC] w_1x4
#[AC] t_1x4
w_1x4 = 89
t_1x4 = 19


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

