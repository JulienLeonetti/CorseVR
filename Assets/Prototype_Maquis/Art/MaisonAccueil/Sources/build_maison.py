import bpy, math, random, os, json
from mathutils import Vector
ROOT=os.path.abspath('work/maison_depart/models');os.makedirs(ROOT,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
r=random.Random(764930)
def U(p):return Vector((p[0],-p[2],p[1]))
def C(h):
 v=[int(h[i:i+2],16)/255 for i in (0,2,4)]
 return tuple(x/12.92 if x<=.04045 else ((x+.055)/1.055)**2.4 for x in v)+(1,)
M={}
for name in ['MaisonStone','MaisonMortar','MaisonWood','MaisonTile','MaisonFloor','MaisonIron']:
 m=bpy.data.materials.new(name);m.diffuse_color=(.5,.45,.35,1);M[name]=m

# Rounded, uneven granite blocks share one bevelled topology, not smooth spheres.
bpy.ops.mesh.primitive_cube_add(size=1);template=bpy.context.object
bevel=template.modifiers.new('Soft_eroded_edges','BEVEL');bevel.width=.065;bevel.segments=2
bpy.ops.object.modifier_apply(modifier=bevel.name)
stone_vertices=[v.co.copy() for v in template.data.vertices];stone_faces=[list(p.vertices) for p in template.data.polygons]
bpy.data.objects.remove(template,do_unlink=True)
class Buffer:
 def __init__(self,mat):self.v=[];self.f=[];self.colors=[];self.mat=mat
 def face(self,pts,col):
  start=len(self.v);self.v.extend([tuple(U(p)) for p in pts]);self.f.append(tuple(range(start,start+len(pts))));self.colors.append(col)
 def box(self,p,size,col,stone=False):
  if stone:
   verts=[]
   for a in stone_vertices:
    verts.append((p[0]+a.x*size[0]+r.uniform(-.008,.008),p[1]+a.z*size[1]+r.uniform(-.008,.008),p[2]-a.y*size[2]+r.uniform(-.008,.008)))
   for inds in stone_faces:self.face([verts[j] for j in inds],col)
  else:
   x,y,z=p;a,b,c=[s*.5 for s in size]
   v=[(x-a,y-b,z-c),(x+a,y-b,z-c),(x+a,y+b,z-c),(x-a,y+b,z-c),(x-a,y-b,z+c),(x+a,y-b,z+c),(x+a,y+b,z+c),(x-a,y+b,z+c)]
   for f in [(0,3,2,1),(4,5,6,7),(0,4,7,3),(1,2,6,5),(3,7,6,2),(0,1,5,4)]:self.face([v[i] for i in f],col)
 def object(self,name):
  mesh=bpy.data.meshes.new(name);mesh.from_pydata(self.v,[],self.f);mesh.update();obj=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(obj);mesh.materials.append(M[self.mat]);attr=mesh.color_attributes.new(name='Color',type='FLOAT_COLOR',domain='CORNER');uv=mesh.uv_layers.new(name='UVMap')
  for poly,col in zip(mesh.polygons,self.colors):
   for li in poly.loop_indices:
    attr.data[li].color=col;p=mesh.vertices[mesh.loops[li].vertex_index].co;uv.data[li].uv=(p.x*.38,p.z*.6+p.y*.12)
  return obj

stone=Buffer('MaisonStone');mortar=Buffer('MaisonMortar');wood=Buffer('MaisonWood');tile=Buffer('MaisonTile');floor=Buffer('MaisonFloor');iron=Buffer('MaisonIron')
palette=['A29B8A','B1A48D','AAA491','B5AA92','939387','BAB09B','9F9B8B']
def holesplit(lo,hi,holes):
 spans=[(lo,hi)]
 for a,b in holes:
  new=[]
  for x,y in spans:
   if b<=x or a>=y:new.append((x,y));continue
   if a>x:new.append((x,a))
   if b<y:new.append((b,y))
  spans=new
 return spans
def wall(axis,constant,lo,hi,windows,door=False):
 y=0
 while y<3.13:
  height=min(r.uniform(.25,.38),3.13-y);cuts=[]
  if door and y<2.57:cuts.append((-.96,.96))
  for a,b,bottom,top in windows:
   if y<top and y+height>bottom:cuts.append((a,b))
  for start,end in holesplit(lo,hi,cuts):
   mid=(start+end)*.5
   # Inset continuous core stops cracks between stones becoming holes through walls.
   p=(mid,y+height*.5,constant) if axis=='x' else (constant,y+height*.5,mid)
   sz=(end-start,height,.30) if axis=='x' else (.30,height,end-start)
   mortar.box(p,sz,C('938D7D'))
   x=start
   while x<end-.005:
    width=min(r.uniform(.35,.88),end-x);p=(x+width*.5,y+height*.5,constant) if axis=='x' else (constant,y+height*.5,x+width*.5)
    sz=(max(.03,width-.012),height-.012,r.uniform(.48,.55)) if axis=='x' else (r.uniform(.48,.55),height-.012,max(.03,width-.012))
    stone.box(p,sz,C(r.choice(palette)),True);x+=width
  y+=height
wall('x',0,-3.15,3.15,[(-2.55,-1.55,1.5,2.55)],True)
wall('x',-5.9,-3.15,3.15,[])
wall('z',-3.15,-5.9,0,[(-4.15,-2.9,1.45,2.6)])
wall('z',3.15,-5.9,0,[])
# Heavy door lintel and deep window sills, as in the photographic reference.
stone.box((0,2.72,0),(2.43,.30,.66),C('BCAD93'),True)
stone.box((-2.05,1.46,0),(1.24,.14,.70),C('B4A58D'),True)
stone.box((-3.15,1.42,-3.525),(.70,.14,1.48),C('B4A58D'),True)
for z in [0,-5.9]:
 mortar.face([(-3.15,3.1,z-.13),(0,4.18,z-.13),(3.15,3.1,z-.13)],C('A49D8A'))
 mortar.face([(3.15,3.1,z+.13),(0,4.18,z+.13),(-3.15,3.1,z+.13)],C('A49D8A'))
 for row in range(4):
  y=3.15+row*.255;width=6.3*max(0,(4.2-y)/1.1)
  for a,b in holesplit(-width*.5,width*.5,[]):
   x=a
   while x<b-.02:
    w=min(r.uniform(.4,.7),b-x);stone.box((x+w*.5,y,z),(w-.01,.245,.50),C(r.choice(palette)),True);x+=w
# Flagstones and worn entrance step: mostly level for comfortable walking.
floor.box((0,-.025,-2.85),(6.23,.06,6.34),C('9C9482'))
for row in range(9):
 z=-5.85+row*.72;x=-3.0
 while x<3:
  w=min(r.uniform(.60,1.10),3-x);floor.box((x+w*.5,0,z), (w-.012,.09,.71), C(r.choice(['B3A68C','BBAF98','A89F8C','C1B49A'])),True);x+=w
floor.box((0,-.005,.48),(2.1,.09,.92),C('B8AD96'),True)
# Exposed rafters, roof underside and barrel tiles, no low crossbeam over the player.
for side in [-1,1]:
 wood.face([(0,4.12,-6.22),(side*3.52,2.98,-6.22),(side*3.52,2.98,.38),(0,4.12,.38)],C('806B4D'))
 for j in range(6):
  z=-5.65+j*1.08
  # sloping structural rafters, visibly irregular edges
  wood.face([(0,4.05,z-.08),(side*3.3,2.99,z-.08),(side*3.3,2.85,z+.08),(0,3.91,z+.08)],C('65563E'))
  wood.face([(0,3.91,z+.08),(side*3.3,2.85,z+.08),(side*3.3,2.99,z-.08),(0,4.05,z-.08)],C('746247'))
 for col in range(30):
  z=-6.20+col*.22
  for row in range(10):
   x0=side*(row*.35);x1=side*((row+1)*.35+.07);colr=C(r.choice(['AC7C5A','B48A68','957052','B08462','BA9475']))
   def point(x,a):return (x,4.23-abs(x)*.325+math.sin(a)*.08,z+math.cos(a)*.106)
   for k in range(6):
    a=k*math.pi/6;b=(k+1)*math.pi/6;tile.face([point(x0,a),point(x1,a),point(x1,b),point(x0,b)],colr)
wood.box((0,3.96,-2.9),(.22,.22,6.6),C('6D5B42'))
# Small stone hearth on the right, with a deeply recessed dark opening.
stone.box((2.56,.14,-1.65),(1.15,.25,1.7),C('A39A85'),True)
for z in [-2.38,-.92]:stone.box((2.64,.73,z),(1.0,1.25,.22),C('AAA18C'),True)
stone.box((2.64,1.42,-1.65),(1.12,.25,1.75),C('AAA18C'),True)
stone.box((2.85,2.25,-1.65),(.62,1.50,1.38),C('A69D88'),True)
iron.box((3.04,.78,-1.65),(.04,1.05,1.25),C('242520'))
for i in range(4):wood.box((2.64,.34,-2.06+i*.27),(.62,.12,.14),C('524835'))
# Table and bench along the left wall; centre of room stays free.
wood.box((-2.25,.80,-3.55),(.87,.10,2.1),C('8B7757'))
for x in [-2.55,-1.98]:
 for z in [-4.35,-2.75]:wood.box((x,.39,z),(.11,.78,.11),C('786448'))
wood.box((-1.50,.43,-3.55),(.34,.09,2.05),C('8D7858'))
for z in [-4.32,-2.78]:wood.box((-1.50,.21,z),(.23,.42,.10),C('786448'))
# Simple timber shutters opened against the exterior beside the windows.
for x in [-2.83,-1.28]:
 for i in range(3):wood.box((x+i*.09,2.02,.38),(.084,1.09,.055),C('777B68'))
# Rear wall shelf with a couple of terracotta vessels.
wood.box((-1.8,1.28,-5.57),(1.7,.10,.42),C('8B7757'))
for index,(x,size) in enumerate([(-2.25,.15),(-1.75,.19)]):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=16,ring_count=10,radius=1,location=U((x,1.45+size*.5,-5.48)))
 ob=bpy.context.object;ob.name='Poterie_'+str(index);ob.scale=(size,size,size*1.25);bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);ob.data.materials.append(M['MaisonTile']);attr=ob.data.color_attributes.new(name='Color',type='FLOAT_COLOR',domain='CORNER')
 for poly in ob.data.polygons:
  poly.use_smooth=True
  for li in poly.loop_indices:attr.data[li].color=C('9C7250')
# Export complete static house separately from the movable door.
for name,buf in [('Maconnerie',stone),('Joints',mortar),('Charpente_Mobilier',wood),('Tuiles',tile),('Dallage',floor),('Foyer_Sombre',iron)]:buf.object(name)
house=list(bpy.context.scene.objects)
def export(objects,name):
 bpy.ops.object.select_all(action='DESELECT')
 for ob in objects:ob.select_set(True)
 bpy.context.view_layer.objects.active=objects[0]
 bpy.ops.export_scene.fbx(filepath=os.path.join(ROOT,name+'.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,global_scale=1,bake_anim=False,use_triangles=True,mesh_smooth_type='FACE')
export(house,'Maison_Corse_Accueil')
door=Buffer('MaisonWood');hardware=Buffer('MaisonIron')
for i in range(8):door.box(((i+.5)*1.82/8,1.255,0),(1.82/8-.005,2.49,.105),C(['86724F','8B7754','7B694A','93805B'][i%4]))
for y in [.35,2.16]:
 hardware.box((.91,y,-.063),(1.72,.075,.026),C('484839'));hardware.box((.91,y,.063),(1.72,.075,.026),C('484839'))
for y in [.35,2.16]:
 for x in [.12,.56,1.15,1.65]:hardware.box((x,y,-.083),(.026,.026,.014),C('5A5949'))
hardware.box((1.56,1.13,-.088),(.045,.24,.04),C('47483B'));hardware.box((1.56,1.24,-.12),(.045,.05,.08),C('47483B'));hardware.box((1.56,1.02,-.12),(.045,.05,.08),C('47483B'))
doorob=door.object('Porte_Planches');ironob=hardware.object('Ferrures');export([doorob,ironob],'Porte_Accueil')
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT,'Maison_Corse_Accueil.blend'))
print('MAISON_COMPLETE',sum(len(o.data.polygons) for o in house if o.type=='MESH'))
