import os
source=open('work/maison_depart/build_maison.py',encoding='utf-8-sig').read()
source=source.replace("work/maison_depart/models","work/maison_art/models")
source=source.replace("bevel.width=.065;bevel.segments=2","bevel.width=.105;bevel.segments=3")
source=source.replace("r.uniform(-.008,.008)","r.uniform(-.014,.014)")
source=source.replace('  if stone:\n','  if stone or self.mat==\'MaisonWood\':\n')
source=source.replace("'MaisonIron']","'MaisonIron','MaisonLinen','MaisonWicker','MaisonCopper','MaisonLeaves','MaisonPlaster','MaisonSoot','MaisonGlass']")
source=source.replace("palette=['A29B8A','B1A48D','AAA491','B5AA92','939387','BAB09B','9F9B8B']","palette=['9E9B8D','B4AC96','A5A898','B7AA96','858D84','B9B39E','969A8A']")
source=source.replace("r.uniform(.35,.88)","r.uniform(.32,1.04)")
# Softly eroded beam and furniture edges, with different wood colours by piece.
source=source.replace("'86724F','8B7754','7B694A','93805B'","'857054','8D795A','76654D','95815F'")
source=source.replace('wood.box((2.64,.34,-2.06+i*.27)','wood.box((2.64,.24,-2.06+i*.27)')

organic_wall=r'''
def clip_cell(poly,nx,ny,limit):
 if not poly:return []
 result=[]
 for i,p in enumerate(poly):
  q=poly[(i+1)%len(poly)];a=nx*p[0]+ny*p[1]-limit;b=nx*q[0]+ny*q[1]-limit
  if a<=1e-8:result.append(p)
  if (a<0 and b>0) or (a>0 and b<0):
   t=a/(a-b);result.append((p[0]+(q[0]-p[0])*t,p[1]+(q[1]-p[1])*t))
 return result
def subtract_hole(poly,a,b,bottom,top):
 left=clip_cell(poly,1,0,a);right=clip_cell(poly,-1,0,-b)
 mid=clip_cell(clip_cell(poly,-1,0,-a),1,0,b)
 below=clip_cell(mid,0,1,bottom);above=clip_cell(mid,0,-1,-top)
 return [p for p in [left,right,below,above] if len(p)>=3]
def organic_block(poly,axis,constant):
 cx=sum(p[0] for p in poly)/len(poly);cy=sum(p[1] for p in poly)/len(poly)
 if abs(sum(poly[i][0]*poly[(i+1)%len(poly)][1]-poly[(i+1)%len(poly)][0]*poly[i][1] for i in range(len(poly))))<.006:return
 outer=[];inner=[]
 for x,y in poly:
  length=max(.03,math.hypot(x-cx,y-cy));gap=.010
  outer.append((cx+(x-cx)*max(.1,1-gap/length),cy+(y-cy)*max(.1,1-gap/length)))
  inner.append((cx+(x-cx)*max(.1,1-(gap+.028)/length),cy+(y-cy)*max(.1,1-(gap+.028)/length)))
 thick=r.uniform(.49,.58);col=C(r.choice(palette));bulge=r.uniform(.009,.023)
 def xyz(p,d):return (p[0],p[1],constant+d) if axis=='x' else (constant+d,p[1],p[0])
 for side in [-1,1]:
  centre=xyz((cx,cy),side*(thick*.5+bulge))
  for i in range(len(poly)):
   j=(i+1)%len(poly);a=xyz(inner[i],side*thick*.5);b=xyz(inner[j],side*thick*.5)
   # Correct outward winding on both wall orientations.
   tri=[centre,a,b] if (axis=='x')==(side>0) else [centre,b,a]
   stone.face(tri,col)
   bevel=[a,xyz(outer[i],side*(thick*.5-.035)),xyz(outer[j],side*(thick*.5-.035)),b]
   stone.face(bevel if (axis=='x')==(side>0) else list(reversed(bevel)),col)
 for i in range(len(poly)):
  j=(i+1)%len(poly);quad=[xyz(outer[i],-.5*thick+.035),xyz(outer[i],.5*thick-.035),xyz(outer[j],.5*thick-.035),xyz(outer[j],-.5*thick+.035)];stone.face(list(reversed(quad)) if axis=='x' else quad,col)
def wall(axis,constant,lo,hi,windows,door=False):
 holes=[(a,b,bottom,top) for a,b,bottom,top in windows]
 if door:holes.append((-.96,.96,0,2.57))
 # Recessed core follows the true openings.
 for row in range(16):
  bottom=row*.20;top=min(3.13,bottom+.20)
  if bottom>=top:continue
  cuts=[(a,b) for a,b,y0,y1 in holes if bottom<y1 and top>y0]
  for a,b in holesplit(lo,hi,cuts):
   p=((a+b)*.5,(bottom+top)*.5,constant) if axis=='x' else (constant,(bottom+top)*.5,(a+b)*.5)
   size=(b-a,top-bottom,.31) if axis=='x' else (.31,top-bottom,b-a)
   mortar.box(p,size,C('999486'))
 seeds=[];rows=9;columns=math.ceil((hi-lo)/.60)
 for row in range(rows):
  for column in range(columns):
   x=lo+(column+.5)*(hi-lo)/columns+r.uniform(-.18,.18)
   y=(row+.5)*3.13/rows+r.uniform(-.11,.11)
   seeds.append((x,y))
 for sx,sy in seeds:
  cell=[(lo,0),(hi,0),(hi,3.13),(lo,3.13)]
  for tx,ty in seeds:
   if (tx,ty)==(sx,sy) or (tx-sx)**2+(ty-sy)**2>2.7:continue
   cell=clip_cell(cell,tx-sx,ty-sy,(tx*tx+ty*ty-sx*sx-sy*sy)*.5)
   if not cell:break
  if len(cell)<3:continue
  pieces=[cell]
  for hole in holes:
   pieces=[piece for poly in pieces for piece in subtract_hole(poly,*hole)]
  for poly in pieces:organic_block(poly,axis,constant)
'''
start=source.index('def wall(axis,constant,lo,hi,windows,door=False):')
end=source.index("wall('x',0,-3.15",start)
source=source[:start]+organic_wall+'\n'+source[end:]

decor=r'''
from mathutils.bvhtree import BVHTree
linen=Buffer('MaisonLinen');wicker=Buffer('MaisonWicker');copper=Buffer('MaisonCopper');herbs=Buffer('MaisonLeaves');soot=Buffer('MaisonSoot');glass=Buffer('MaisonGlass')

def lathe(buf,centre,profile,col,segments=32):
 for j in range(len(profile)-1):
  ra,ya=profile[j];rb,yb=profile[j+1]
  for i in range(segments):
   a=i*math.tau/segments;b=(i+1)*math.tau/segments
   def p(radius,y,angle):return (centre[0]+radius*math.cos(angle),centre[1]+y,centre[2]+radius*math.sin(angle))
   buf.face([p(ra,ya,a),p(rb,yb,a),p(rb,yb,b),p(ra,ya,b)],col)

def tube(buf,points,radius,col,sides=6):
 # Sweep a small tube through a curved polyline, used for handles and woven fibres.
 for j in range(len(points)-1):
  a=Vector(points[j]);b=Vector(points[j+1]);d=(b-a).normalized();side=d.cross(Vector((0,1,0)))
  if side.length<.05:side=d.cross(Vector((1,0,0)))
  side.normalize();other=d.cross(side).normalized()
  for i in range(sides):
   t=i*math.tau/sides;t2=(i+1)*math.tau/sides
   off=side*math.cos(t)*radius+other*math.sin(t)*radius;off2=side*math.cos(t2)*radius+other*math.sin(t2)*radius
   buf.face([tuple(a+off),tuple(b+off),tuple(b+off2),tuple(a+off2)],col)

# Two open ceramic vessels, with actual rims and hollow necks.
for ob in list(bpy.context.scene.objects):
 if ob.name.startswith('Poterie_'):bpy.data.objects.remove(ob,do_unlink=True)
for x,h,w in [(-2.25,.32,.15),(-1.75,.40,.18)]:
 lathe(tile,(x,1.335,-5.48),[(0,0),(w*.6,0),(w,h*.28),(w*.9,h*.65),(w*.38,h*.86),(w*.38,h),(w*.28,h),(w*.28,h*.86),(w*.75,h*.62),(w*.80,h*.28),(w*.45,.02),(0,.02)],C('A6825D'))
lathe(tile,(-2.32,.86,-3.80),[(0,0),(.10,0),(.17,.13),(.16,.27),(.075,.35),(.075,.41),(.061,.41),(.061,.35),(.13,.25),(.13,.13),(.07,.025)],C('AF805D'))
tube(tile,[(-2.23,1.23,-3.80),(-2.05,1.22,-3.80),(-2.04,1.03,-3.80),(-2.18,.99,-3.80)],.025,C('AC7D59'),8)
lathe(tile,(-2.26,.86,-2.96),[(0,0),(.07,0),(.14,.03),(.19,.14),(.18,.17),(.165,.17),(.165,.145),(.11,.055),(0,.025)],C('B18C68'))
lathe(tile,(-2.18,.86,-4.28),[(0,0),(.04,0),(.055,.04),(.06,.15),(.05,.16),(.043,.145),(.043,.05),(0,.02)],C('827B5C'),24)

# A folded handwoven linen runner drapes over the outer end of the table.
for j in range(18):
 for i in range(12):
  def pt(a,b):
   x=-2.69+a*.075;z=-4.61+b*.052
   y=.861+.006*math.sin(a*1.4+b*.5)
   if z< -4.60:y-=(-4.60-z)*3.8
   return (x,y,z)
  col=C('C6BDA1' if i not in [2,3,9] else '7D8A7C');linen.face([pt(i,j),pt(i+1,j),pt(i+1,j+1),pt(i,j+1)],col)

# Wicker basket beside the table, woven strips crossing at an irregular angle.
cx,cy,cz=-2.30,.09,-4.95
lathe(wicker,(cx,cy,cz),[(0,0),(.20,0),(.29,.39),(.28,.41),(.26,.39),(.19,.03),(0,.03)],C('8D7951'),32)
for row in range(17):
 y=row*.023;rad=.20+.09*(y/.4)
 points=[(cx+rad*math.cos(i*math.tau/48),cy+y,cz+rad*math.sin(i*math.tau/48)) for i in range(49)]
 tube(wicker,points,.007,C(['AD9564','BAA26D','9D885C'][row%3]),4)
for i in range(34):
 a=i*math.tau/34
 pts=[]
 for j in range(18):
  y=j*.023;rad=.204+.09*(y/.4)+.006*math.sin(j*math.pi)
  pts.append((cx+rad*math.cos(a),cy+y,cz+rad*math.sin(a)))
 tube(wicker,pts,.005,C('B49D6D'),4)
tube(wicker,[(cx+.26*math.cos(a),cy+.4+.27*math.sin(a),cz) for a in [i*math.pi/24 for i in range(25)]],.015,C('A58D5D'),6)

# Small woven rug, flat enough to preserve the level walking surface.
for j in range(32):
 for i in range(20):
  x=-.72+i*.072;z=-4.35+j*.057
  border=i<2 or i>17 or j<3 or j>28;col=C('7D8980' if border else ('B0A58A' if (i+j)%5 else 'A09479'))
  linen.face([(x,.057,z),(x+.072,.057,z),(x+.072,.057,z+.057),(x,.057,z+.057)],col)
for i in range(28):
 x=-.71+i*.052;tube(linen,[(x,.058,-4.35),(x+r.uniform(-.015,.015),.055,-4.44)],.003,C('B6AC94'),3)

# Copper cooking pot and hanging utensils at the hearth, with understated metal highlights.
lathe(copper,(2.48,.32,-1.65),[(0,0),(.17,0),(.24,.06),(.26,.29),(.27,.31),(.25,.32),(.235,.29)],C('9F6846'))
lathe(soot,(2.48,.32,-1.65),[(.235,.29),(.215,.075),(.15,.05)],C('796149'))
soot.face([(2.48+.15*math.cos(i*math.tau/32),.37,-1.65+.15*math.sin(i*math.tau/32)) for i in range(31,-1,-1)],C('796149'))
copper.face([(2.48+.17*math.cos(i*math.tau/32),.32,-1.65+.17*math.sin(i*math.tau/32)) for i in range(32)],C('9F6846'))
tube(iron,[(2.48+.28*math.cos(a),.63+.28*math.sin(a),-1.65) for a in [i*math.pi/24 for i in range(25)]],.012,C('4E5148'))
wood.box((2.84,1.9,-3.05),(.10,.09,1.05),C('74644B'))
for k in range(3):
 z=-3.45+k*.34;tube(iron,[(2.75,1.88,z),(2.75,1.69,z),(2.73,1.65,z)],.009,C('575B50'))
 tube(copper,[(2.72,1.68,z),(2.70,1.35,z)],.012,C('967451'))
 lathe(copper,(2.69,1.23,z),[(0,0),(.055,.015),(.075,.07),(.064,.10),(.051,.085),(0,.03)],C('A47950'),16)

# An old chest against the rear wall. Central path remains open.
wood.box((1.95,.36,-5.29),(1.35,.60,.68),C('7D6D52'),True)
wood.box((1.95,.70,-5.29),(1.41,.12,.72),C('8C7B5D'),True)
for x in [1.46,2.44]:iron.box((x,.39,-4.935),(.055,.54,.025),C('51554A'))
iron.box((1.95,.54,-4.92),(.09,.15,.04),C('4C5045'))

# Real window joinery, with four transparent panes in each existing opening.
for x in [-2.515,-1.585]:wood.box((x,2.025,-.035),(.07,1.05,.13),C('82745A'))
for y in [1.535,2.515]:wood.box((-2.05,y,-.035),(1.0,.07,.13),C('8D7B5C'))
wood.box((-2.05,2.025,-.04),(.04,.96,.11),C('8D7B5C'))
wood.box((-2.05,2.025,-.04),(.94,.035,.11),C('8D7B5C'))
for a,b in [(-2.48,-2.075),(-2.025,-1.62)]:
 for y0,y1 in [(1.57,2.005),(2.045,2.48)]:
  glass.face([(a,y0,-.015),(b,y0,-.015),(b,y1,-.015),(a,y1,-.015)],C('DAE8E7'))
for z in [-4.115,-2.935]:wood.box((-3.15,2.025,z),(.13,1.15,.07),C('82745A'))
for y in [1.485,2.565]:wood.box((-3.15,y,-3.525),(.13,.07,1.25),C('8D7B5C'))
wood.box((-3.15,2.025,-3.525),(.11,1.06,.04),C('8D7B5C'))
wood.box((-3.15,2.025,-3.525),(.11,.035,1.18),C('8D7B5C'))
for a,b in [(-4.08,-3.55),(-3.50,-2.97)]:
 for y0,y1 in [(1.52,2.005),(2.045,2.53)]:
  glass.face([(-3.15,y0,a),(-3.15,y0,b),(-3.15,y1,b),(-3.15,y1,a)],C('DAE8E7'))

# A framed relief silhouette of Corsica, made as geometry, on the rear wall.
wood.box((.0,1.99,-5.57),(1.12,1.55,.055),C('77664D'))
linen.box((.0,1.99,-5.53),(.98,1.41,.015),C('BDB398'))
outline=[(.02,.66),(.12,.61),(.11,.48),(.23,.37),(.28,.16),(.19,-.03),(.17,-.24),(.04,-.45),(-.01,-.61),(-.14,-.49),(-.20,-.37),(-.29,-.26),(-.25,-.14),(-.34,.03),(-.28,.16),(-.34,.29),(-.22,.38),(-.10,.42),(-.05,.57)]
centre=(0,1.99,-5.505)
for i in range(len(outline)):
 a=outline[i];b=outline[(i+1)%len(outline)];herbs.face([centre,(-a[0],1.99+a[1],-5.50),(-b[0],1.99+b[1],-5.50)],C('6C7C6B'))

# Remove the former intersecting plaster sheets; the existing stone wall stays intact.
for n,buf in [('Linge_Tisse',linen),('Vannerie',wicker),('Cuivres',copper),('Carte_Corse',herbs),('Fond_Marmite',soot),('Vitres_Fenetres',glass)]:buf.object(n)

def bake_contact_ao(objects):
 # Portable geometric AO stored in vertex alpha, usable by the Unity material.
 import bmesh
 for ob in objects:
  if ob.type!='MESH' or ob.name not in ['Cuivres','Vannerie','Tuiles','Fond_Marmite']:continue
  bm=bmesh.new();bm.from_mesh(ob.data);bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=.00001);bm.to_mesh(ob.data);bm.free();ob.data.update()
  for poly in ob.data.polygons:
   if ob.name!='Tuiles' or poly.center.z<2:poly.use_smooth=True
  if hasattr(ob.data,'set_sharp_from_angle'):ob.data.set_sharp_from_angle(angle=math.radians(42))
 verts=[];polys=[]
 for ob in objects:
  if ob.type!='MESH' or ob.name=='Vitres_Fenetres':continue
  offset=len(verts);verts.extend([ob.matrix_world@v.co for v in ob.data.vertices]);polys.extend([tuple(offset+i for i in p.vertices) for p in ob.data.polygons])
 bvh=BVHTree.FromPolygons(verts,polys,all_triangles=False)
 rays=[]
 for k in range(10):
  u=(k+.5)/10;a=k*2.399963;rxy=math.sqrt(u);rays.append(Vector((math.cos(a)*rxy,math.sin(a)*rxy,math.sqrt(1-u))))
 for ob in objects:
  if ob.type!='MESH' or ob.name=='Vitres_Fenetres':continue
  mesh=ob.data;attr=mesh.color_attributes.get('Color')
  if attr is None:continue
  for p in mesh.polygons:
   normal=(ob.matrix_world.to_3x3()@p.normal).normalized();centre=ob.matrix_world@p.center
   side=normal.cross(Vector((0,0,1)))
   if side.length<.1:side=normal.cross(Vector((0,1,0)))
   side.normalize();other=normal.cross(side);occluded=0
   for ray in rays:
    direction=side*ray.x+other*ray.y+normal*ray.z
    hit,_,_,dist=bvh.ray_cast(centre+normal*.018,direction,1.1)
    if hit is not None:occluded+=max(0,1-dist/1.1)*.85+.15
   ao=max(.38,1-.65*occluded/len(rays))
   for li in p.loop_indices:
    old=attr.data[li].color;attr.data[li].color=(old[0],old[1],old[2],ao)
 print('CONTACT_AO_COMPLETE',len(objects))
'''
source=source.replace('# Export complete static house separately from the movable door.',decor+'\n# Export complete static house separately from the movable door.')
source=source.replace("export(house,'Maison_Corse_Accueil')","bake_contact_ao(house)\nexport(house,'Maison_Corse_Accueil')")
exec(compile(source,'maison_art_generated','exec'))
