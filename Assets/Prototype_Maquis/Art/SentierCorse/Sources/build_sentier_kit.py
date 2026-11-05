import os, math, random, json
import bpy
from mathutils import Vector

# Reuse the local FBX exporter and curved-leaf mesh utilities, not the old plants.
source = r'C:\Users\jeanj\CorseVR\Assets\Prototype_Maquis\Art\Bavella\Sources\build_corsican_kit.py'
exec(open(source, encoding='utf-8-sig').read().split('stats=[]')[0])
ROOT = os.path.abspath('work/sentier_kit/models')
os.makedirs(ROOT, exist_ok=True)
stats=[]

def rounded_leaf(self,p,axis,L,W,col,serrated=False,mat=1):
 p=Vector(p);d=Vector(axis).normalized();side=d.cross(Vector((0,0,1))).normalized()
 if side.length<.1:side=Vector((1,0,0))
 normal=d.cross(side).normalized()
 # Rounded outline with a short tip, rather than a diamond-shaped blade.
 outline=[(0,0),(.18,-.72),(.50,-1),(.79,-.72),(1,0),(.79,.72),(.5,1),(.18,.72)]
 center=p+d*(L*.5)+normal*(L*.045)
 for i in range(8):
  u,v=outline[i];u2,v2=outline[(i+1)%8]
  a=p+d*(L*u)+side*(W*v)+normal*(math.sin(u*math.pi)*L*.025)
  b=p+d*(L*u2)+side*(W*v2)+normal*(math.sin(u2*math.pi)*L*.025)
  self.face([a,b,center],col,mat,[(.5+v*.5,u),(.5+v2*.5,u2),(.5,.5)])
Geo.leaf=rounded_leaf

def color_mix(hexcode, scale):
 c=C(hexcode)
 return tuple(min(1,x*scale) if i<3 else 1 for i,x in enumerate(c))

def canopy(g, center, radii, count, seed, species, low=False):
 r=random.Random(seed)
 # Leaves occupy a soft, irregular volume rather than repeated radial fans.
 for i in range(count):
  theta=r.uniform(0,math.tau); z=r.uniform(-1,1); rr=r.random()**.40
  p=Vector((math.cos(theta)*math.sqrt(1-z*z)*radii[0],math.sin(theta)*math.sqrt(1-z*z)*radii[1],z*radii[2]))*rr+center
  a=r.uniform(0,math.tau)
  axis=Vector((math.cos(a),math.sin(a),r.uniform(-.42,.62)))
  length=r.uniform(.15,.245) if species!='Ciste' else r.uniform(.075,.13)
  if low: length*=1.48
  palette={'Chene':['486D39','577E40','648647','3C6138'], 'Arbousier':['426F42','597E43','6E9048','3B603C'], 'Lentisque':['587D3D','6B8844','466A38','78934E'], 'Ciste':['7B945A','8E9B64','647F50']}[species]
  col=color_mix(palette[i%len(palette)],r.uniform(.78,1.14))
  g.leaf(p,axis,length,length*(.32 if species=='Chene' else .28 if species=='Arbousier' else .21),col,species in ['Chene','Arbousier'])
  if i%19==0:
   g.tube(center.lerp(p,.6),p,.003,C('63523B'),.01,3,1)
  if species=='Ciste' and i%31==0 and p.z>center.z:
   g.flower(p,.038)

def broad_tree(species,v,low):
 random.seed(8945+v*71+(0 if species=='Chene' else 400))
 g=Geo();g.low=True  # four triangles per curved leaf, including high LOD
 height=(6.9+v*.48) if species=='Chene' else 4.5+v*.5
 wood=C('676252' if species=='Chene' else '775943')
 trunk=[]
 for i in range(8):
  t=i/7;trunk.append(Vector((math.sin(t*3.2+v)*.32*t,math.sin(t*4.4)*.21*t,t*height)))
 for i in range(7):
  radius=(.24 if species=='Chene' else .17)*(1-i/9)**1.2
  next_radius=(.24 if species=='Chene' else .17)*(1-(i+1)/9)**1.2
  g.tube(trunk[i],trunk[i+1],radius,wood,.014,9 if not low else 6,2,next_radius/radius)
 # Unequal boughs with curved secondary forks create the visible silhouette.
 clusters=[]
 for j in range(10 if species=='Chene' else 8):
  a=j*2.399+v*.8+random.uniform(-.32,.32)
  f=.39+j*.05; base=trunk[min(6,int(f*7))]
  radial=Vector((math.cos(a),math.sin(a),0))
  reach=random.uniform(1.5,2.25)*(1.1 if species=='Chene' else .78)
  mid=base+radial*reach*.50+Vector((0,0,.33))
  tip=base+radial*reach+Vector((0,0,random.uniform(.7,1.35)))
  g.tube(base,mid,.095*(1-j*.055),wood,.09,7,3,.66)
  g.tube(mid,tip,.061*(1-j*.055),wood,.08,6,3,.18)
  for k in range(3):
   offset=Vector((math.cos(a+(k-1)*.68),math.sin(a+(k-1)*.68),.48))
   end=mid.lerp(tip,.5+k*.22)+offset*random.uniform(.30,.72)
   start=mid.lerp(tip,.15+k*.20)
   g.tube(start,end,.023,wood,.05,5,2,.14)
   clusters.append((end,Vector((random.uniform(.58,.85),random.uniform(.52,.78),random.uniform(.5,.72)))))
 for k,(p,rad) in enumerate(clusters):
  canopy(g,p,rad,32 if low else 100,801+k*11+v*400,species,low)
 canopy(g,trunk[-1]+Vector((.1,0,-.1)),Vector((.95,.9,.75)),45 if low else 160,70+v,species,low)
 # Flared root collar without large exposed roots across the path.
 for j in range(5):
  a=j*math.tau/5+.2*v
  g.tube(Vector((math.cos(a)*.4,math.sin(a)*.4,-.06)),Vector((0,0,.4)),.085,wood,.03,5,2,.7)
 return g.object(species+'_'+str(v)+('_Low' if low else '_High'))

for species,n in [('Chene',3),('Arbousier',2)]:
 for v in range(n):
  for low in [False,True]:stats.append(export(broad_tree(species,v,low)))

def bush(species,v,low):
 random.seed(705+v*55+(150 if species=='Ciste' else 0));g=Geo();g.low=True
 H=.8 if species=='Ciste' else 1.5
 clusters=[]
 for j in range(6):
  a=j*2.399+v;start=Vector((random.uniform(-.10,.10),random.uniform(-.10,.10),-.02))
  end=Vector((math.cos(a)*random.uniform(.32,.65),math.sin(a)*random.uniform(.32,.65),H*random.uniform(.60,1)))
  g.tube(start,end,.018 if species=='Ciste' else .035,C('726044'),.07,5,3,.2)
  for k in range(2):
   tip=end+Vector((math.cos(a+k*1.3)*.22,math.sin(a+k*1.3)*.22,.13))
   g.tube(end*.7,tip,.008,C('726044'),.03,4,2)
   clusters.append(tip)
 for k,p in enumerate(clusters):
  radii=Vector((.30,.30,.23)) if species=='Ciste' else Vector((.47,.40,.40))
  canopy(g,p,radii,12 if low else 47,554+k*23+v*88,species,low)
 return g.object(species+'_'+str(v)+('_Low' if low else '_High'))
for species,n in [('Lentisque',3),('Ciste',2)]:
 for v in range(n):
  for low in [False,True]:stats.append(export(bush(species,v,low)))

for v in range(4):
 random.seed(55+v)
 bpy.ops.mesh.primitive_cube_add(size=2)
 ob=bpy.context.object;ob.name='Granit_Anguleux_'+str(v)
 ob.scale=(1,.75,.7);bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 bevel=ob.modifiers.new('Erosion_des_aretes','BEVEL');bevel.width=.20+v*.015;bevel.segments=2
 bpy.context.view_layer.objects.active=ob;bpy.ops.object.modifier_apply(modifier=bevel.name)
 for ve in ob.data.vertices:
  p=ve.co
  p.x+=.13*math.sin(p.y*4.7+p.z*3+v)+random.uniform(-.055,.055)
  p.y+=.10*math.sin(p.x*5+p.z*3+v)
  p.z+=.09*math.sin(p.x*5+p.y*2+v)
  p.z+=.74
 ob.data.materials.append(MATS['Granite'])
 ca=ob.data.color_attributes.new(name='Color',type='FLOAT_COLOR',domain='CORNER')
 for poly in ob.data.polygons:
  col=C(['AEADA0','B4AD9B','B9B1A2','A8AC9B'][(poly.index+v)%4])
  for li in poly.loop_indices:ca.data[li].color=col
  poly.use_smooth=False
 stats.append(export(ob))

for ob in bpy.data.objects:ob.hide_set(False)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT,'Sentier_Corse_Kit.blend'))
with open(os.path.join(ROOT,'kit_stats.json'),'w') as f:json.dump(stats,f,indent=2)
print('SENTIER_KIT_COMPLETE',len(stats),sum(x['triangles'] for x in stats))
