import os, math, random, json
import bpy
from mathutils import Vector
source=r'C:\Users\jeanj\CorseVR\Assets\Prototype_Maquis\Art\Bavella\Sources\build_corsican_kit.py'
exec(open(source,encoding='utf-8-sig').read().split('stats=[]')[0])
ROOT=os.path.abspath('work/sentier_v4/models');os.makedirs(ROOT,exist_ok=True)

def leaf(self,p,axis,L,W,col,serrated=False,mat=1):
 p=Vector(p);d=Vector(axis).normalized();side=d.cross(Vector((0,0,1))).normalized()
 if side.length<.1:side=Vector((1,0,0))
 normal=d.cross(side).normalized();center=p+d*L*.5+normal*L*.06
 outline=[(0,0),(.3,-.9),(.7,-1),(1,0),(.7,1),(.3,.9)]
 for i in range(6):
  u,v=outline[i];u2,v2=outline[(i+1)%6]
  self.face([p+d*L*u+side*W*v,p+d*L*u2+side*W*v2,center],col,mat,[(.5+v*.5,u),(.5+v2*.5,u2),(.5,.5)])
Geo.leaf=leaf

def tree(v,low,pine=False):
 r=random.Random(43518+v*79+(900 if pine else 0));g=Geo();g.low=True
 H=(10.8+v*.9) if pine else (8.8+v*.8); wood=C('756A56' if not pine else '8B7059')
 stem=[]
 for i in range(12):
  t=i/11;stem.append(Vector((.38*math.sin(t*2.2+v)*t,.27*math.sin(t*3.8+v*.6)*t,t*H)))
 sides=7 if low else 11
 rings=[]
 for i,p in enumerate(stem):
  rad=.32*(1-i/13)**1.4
  rings.append([p+Vector((math.cos(j*math.tau/sides)*rad,math.sin(j*math.tau/sides)*rad,0)) for j in range(sides)])
 for i in range(11):
  for j in range(sides):
   g.face([rings[i][j],rings[i][(j+1)%sides],rings[i+1][(j+1)%sides],rings[i+1][j]],wood,0,[(j/sides,i*.4),((j+1)/sides,i*.4),((j+1)/sides,(i+1)*.4),(j/sides,(i+1)*.4)])
 clusters=[]
 for j in range(11 if pine else 9):
  # Branch origins and asymmetrical fork angles are deliberately irregular.
  t=r.uniform(.59,.91) if pine else r.uniform(.36,.84)
  base=stem[int(t*11)];ang=j*2.399+r.uniform(-.6,.6)+v
  radial=Vector((math.cos(ang),math.sin(ang),0));reach=r.uniform(1.5,3.2)*(1 if pine else 1.15)
  mid=base+radial*reach*.4+Vector((0,0,.3));end=base+radial*reach+Vector((0,0,r.uniform(.65,1.6)))
  thick=.10*(1-t)+.055
  g.tube(base,mid,thick,wood,.12,6 if low else 8,3,.66)
  g.tube(mid,end,thick*.66,wood,-.10,6,3,.15)
  for k in range(3):
   start=mid.lerp(end,r.uniform(.3,.9));a=ang+r.uniform(-1.4,1.4)
   tip=start+Vector((math.cos(a),math.sin(a),r.uniform(.3,.9)))*r.uniform(.45,.95)
   g.tube(start,tip,.024,wood,.05,4 if low else 6,2,.1)
   clusters.append((tip,Vector((r.uniform(.7,1.1),r.uniform(.65,.95),r.uniform(.30,.48) if pine else r.uniform(.55,.85)))))
 clusters.append((stem[-1],Vector((1,1,.45 if pine else .8))))
 palette=['456C3B','517A43','66844C','3C623A'] if not pine else ['476B4C','5F7C50','71894F','3F6247']
 for ci,(center,radii) in enumerate(clusters):
  for i in range(55 if low else 230):
   a=r.uniform(0,math.tau);z=r.uniform(-1,1);rr=r.random()**.4
   p=center+Vector((math.cos(a)*math.sqrt(1-z*z)*radii.x,math.sin(a)*math.sqrt(1-z*z)*radii.y,z*radii.z))*rr
   axis=Vector((math.cos(a+r.uniform(-1,1)),math.sin(a+r.uniform(-1,1)),r.uniform(-.3,.7)))
   L=r.uniform(.19,.29) if pine else r.uniform(.12,.20)
   if low:L*=1.7
   col=C(palette[(i+ci)%4]);factor=r.uniform(.85,1.1);col=tuple(c*factor if k<3 else c for k,c in enumerate(col))
   g.leaf(p,axis,L,L*(.12 if pine else .32),col)
 for j in range(7):
  a=j*math.tau/7+v;g.tube(Vector((math.cos(a)*.55,math.sin(a)*.55,-.09)),Vector((0,0,.5)),.11,wood,.04,5,2,.7)
 return g.object(('Pin_Cotier_' if pine else 'Chene_Ancien_')+str(v)+('_Low' if low else '_High'))

stats=[]
for pine,n in [(False,3),(True,2)]:
 for v in range(n):
  for low in [False,True]:stats.append(export(tree(v,low,pine)))
for v in range(5):
 r=random.Random(187+v)
 bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=3,radius=1)
 ob=bpy.context.object;ob.name='Granit_Erode_'+str(v)
 for vert in ob.data.vertices:
  d=vert.co.copy();a=math.atan2(d.y,d.x)
  uneven=1+.13*math.sin(d.x*4.5+v)+.09*math.cos(d.y*5+d.z*3+v)
  # Rounded granite shoulders, asymmetric hollows and a stable buried foot.
  hollow=1-.25*max(0,math.cos(a*3.0+v+d.z*2))**12*max(0,1-abs(d.z)*1.3)
  vert.co=Vector((d.x*uneven*hollow*(1.2 if v%2 else .92),d.y*uneven*hollow*.85,max(-.64,d.z*uneven*.8)+.64))
 ob.data.materials.append(MATS['Granite']);ca=ob.data.color_attributes.new(name='Color',type='FLOAT_COLOR',domain='CORNER')
 for poly in ob.data.polygons:
  poly.use_smooth=True;col=C(['B7B3A4','B4B3A5','AAA99D','BDB5A3','A9ADA0'][v])
  for li in poly.loop_indices:ca.data[li].color=col
 stats.append(export(ob))
for ob in bpy.data.objects:ob.hide_set(False)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT,'Grands_Arbres_Corse.blend'))
with open(os.path.join(ROOT,'stats.json'),'w') as f:json.dump(stats,f,indent=2)
print('TREES_V4_COMPLETE',len(stats))
