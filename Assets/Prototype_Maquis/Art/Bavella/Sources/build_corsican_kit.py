import bpy, math, random, os, json
from mathutils import Vector
ROOT = os.path.abspath("work/bavella_kit")
os.makedirs(ROOT, exist_ok=True)
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
MATS={}
for name,col in [('Wood',(0.39,0.27,0.19,1)),('Leaf',(0.23,0.38,0.15,1)),('Flower',(0.94,0.88,0.71,1)),('Granite',(0.65,0.49,0.40,1))]:
 m=bpy.data.materials.new(name); m.diffuse_color=col; MATS[name]=m
def C(hex):
 vals=[int(hex[i:i+2],16)/255 for i in (0,2,4)]
 return tuple(v/12.92 if v<=.04045 else ((v+.055)/1.055)**2.4 for v in vals)+(1,)
class Geo:
 def __init__(self): self.v=[];self.f=[];self.cols=[];self.uv=[];self.mi=[]
 def face(self,ps,col,mat,uv=None):
  k=len(self.v); self.v.extend([tuple(p) for p in ps]); self.cols.extend([col]*len(ps));self.uv.extend(uv or [(0,0)]*len(ps));self.f.append(tuple(range(k,k+len(ps))));self.mi.append(mat)
 def tube(self,a,b,r,col,bend=.05,sides=6,steps=4,end_factor=.24):
  a,b=Vector(a),Vector(b);d=(b-a).normalized();side=d.cross(Vector((0,1,0))).normalized()
  if side.length<.1:side=Vector((1,0,0))
  other=d.cross(side).normalized()
  def point(t,j):
   an=j/sides*math.tau;n=side*math.cos(an)+other*math.sin(an)
   return a.lerp(b,t)+side*(math.sin(t*math.pi)*bend)+n*r*(1-(1-end_factor)*t)
  for q in range(steps):
   for j in range(sides):
    self.face([point(q/steps,j),point(q/steps,j+1),point((q+1)/steps,j+1),point((q+1)/steps,j)],col,0,[(j/sides,q/steps),( (j+1)/sides,q/steps),((j+1)/sides,(q+1)/steps),(j/sides,(q+1)/steps)])
 def leaf(self,p,axis,L,W,col,serrated=False,mat=1):
  p=Vector(p);d=Vector(axis).normalized();side=d.cross(Vector((0,0,1))).normalized()
  if side.length<.1:side=Vector((1,0,0))
  side=side*math.cos(random.uniform(-.9,.9))+d.cross(side)*math.sin(random.uniform(-.5,.5))
  normal=d.cross(side).normalized();rings=(2 if getattr(self,'low',False) else (6 if serrated else 4))
  centers=[p+d*(L*i/rings)+normal*(math.sin(i/rings*math.pi)*L*.08) for i in range(rings+1)]
  for i in range(rings):
   f1=i/rings;f2=(i+1)/rings
   w1=math.sin(f1*math.pi)*W*(1 if i%2==0 else .90);w2=math.sin(f2*math.pi)*W*(1 if (i+1)%2==0 else .90)
   a=centers[i];b=centers[i+1]
   self.face([a-side*w1,b-side*w2,b+normal*L*.035,a+normal*L*.035],tuple(x*.91 if k<3 else x for k,x in enumerate(col)),mat,[(0,f1),(0,f2),(.5,f2),(.5,f1)])
   self.face([a+normal*L*.035,b+normal*L*.035,b+side*w2,a+side*w1],col,mat,[(.5,f1),(.5,f2),(1,f2),(1,f1)])
 def flower(self,p,sz=.045):
  p=Vector(p);col=C('F8EAD2')
  for j in range(5):
   a=j*math.tau/5;d=Vector((math.cos(a),math.sin(a),.15))
   self.leaf(p+d*sz*.1,d,sz,sz*.35,col,False,2)
  self.face([p+Vector((-.012,-.012,.008)),p+Vector((.012,-.012,.008)),p+Vector((.012,.012,.008)),p+Vector((-.012,.012,.008))],C('D9AA3C'),2,[(0,0)]*4)
 def object(self,name,rock=False):
  mesh=bpy.data.meshes.new(name);mesh.from_pydata(self.v,[],self.f);mesh.update()
  ob=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(ob)
  for key in (['Granite'] if rock else ['Wood','Leaf','Flower']):mesh.materials.append(MATS[key])
  color=mesh.color_attributes.new(name='Color',type='FLOAT_COLOR',domain='CORNER');uv=mesh.uv_layers.new(name='UVMap')
  for poly,mi in zip(mesh.polygons,self.mi):
   poly.material_index=0 if rock else mi;poly.use_smooth=not rock
   for li in poly.loop_indices:
    vi=mesh.loops[li].vertex_index;color.data[li].color=self.cols[vi];uv.data[li].uv=self.uv[vi]
  # Weld shared positions so smooth normals describe curved surfaces, retaining per-corner UVs.
  import bmesh
  bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=.00001);bm.to_mesh(mesh);bm.free();mesh.update()
  return ob
def export(ob):
 bpy.ops.object.select_all(action='DESELECT');ob.select_set(True);bpy.context.view_layer.objects.active=ob
 bpy.ops.export_scene.fbx(filepath=os.path.join(ROOT,ob.name+'.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,global_scale=1,add_leaf_bones=False,bake_anim=False,mesh_smooth_type='FACE',use_triangles=True)
 ob.hide_set(True)
 return {'name':ob.name,'vertices':len(ob.data.vertices),'triangles':sum(len(p.vertices)-2 for p in ob.data.polygons)}
stats=[]
def shrub(spec,variant,lod):
 random.seed(1200+spec*70+variant*17)
 g=Geo();g.low=lod; H=[1.4,2.3,.78][spec];W=[.83,.86,.52][spec];main=([7,5,6] if lod else [9,7,7])[spec]; wood=C(['746245','A56A49','786A47'][spec]);leafBase=['78914A','60844B','929B62'][spec]
 for i in range(main):
  a=i*2.399+random.uniform(-.3,.3)
  d=Vector((math.cos(a),math.sin(a),0));b=Vector((0,0,.03));end=d*W*random.uniform(.45,1)+Vector((0,0,H*random.uniform(.62,1)))
  g.tube(b,end,[.043,.075,.018][spec]*random.uniform(.7,1.2),wood,random.uniform(-.12,.12),6,5)
  for j in range(3 if lod else 6):
   t=.28+j*(.28 if lod else .13);start=b.lerp(end,min(t,.95))
   an=a+random.uniform(-1.5,1.5);rad=Vector((math.cos(an),math.sin(an),random.uniform(.45,1.3))).normalized()
   tip=start+rad*random.uniform(.24,.46)*H
   g.tube(start,tip,[.018,.03,.008][spec],wood,.04,5,3)
   for k in range(2 if lod else 5):
    root=start.lerp(tip,.18+k*(.5 if lod else .17)); angle=an+random.uniform(-1.8,1.8)
    twig=Vector((math.cos(angle),math.sin(angle),random.uniform(.3,1.4))).normalized()
    length=[.24,.32,.21][spec]*random.uniform(.8,1.3);last=root+twig*length
    g.tube(root,last,[.005,.009,.003][spec],wood,.02,4,2)
    color=list(C(leafBase));factor=random.uniform(.80,1.2); color=tuple(x*factor if n<3 else x for n,x in enumerate(color))
    if spec==0:
     # Paripinnate: four pairs and no terminal leaflet.
     side=twig.cross(Vector((0,0,1))).normalized()
     for pair in range(2 if lod else 4):
      node=root+twig*length*(.18+pair*(.50 if lod else .21))
      for sign in (-1,1):
       ax=(side*sign+twig*.35+Vector((0,0,random.uniform(-.2,.4)))).normalized()
       g.leaf(node,ax,random.uniform(.075,.115),.032,color)
    elif spec==1:
     for n in range(4 if lod else 9):
      t=.10+n/(5 if lod else 10);node=root+twig*length*t
      angle2=angle+n*2.399
      ax=Vector((math.cos(angle2),math.sin(angle2),random.uniform(-.1,.6)))
      g.leaf(node,ax,random.uniform(.15,.23),.055,color,True)
    else:
     side=twig.cross(Vector((0,0,1))).normalized()
     for n in range(3 if lod else 4):
      node=root+twig*length*(.18+n*.22)
      for sign in (-1,1):g.leaf(node,twig*.7+side*sign*.6,.115,.019,color)
     if random.random()<.34:g.flower(last,.035)
 return g.object(['Lentisque','Arbousier','Ciste'][spec]+'_'+str(variant)+('_Low' if lod else '_High'))
for spec in range(3):
 for v in range(3):
  for lod in (0,1):stats.append(export(shrub(spec,v,lod)))
def pine(v,lod):
 random.seed(931+v);g=Geo();g.low=lod;wood=C('88664D');H=7.4+v*.7
 trunk=[Vector((.20*math.sin(j*.7+v),.20*math.sin(j*.5),j*H/8)) for j in range(9)]
 for j in range(8):g.tube(trunk[j],trunk[j+1],.19*(1-j/10),wood,.01,8,2,(1-(j+1)/10)/(1-j/10))
 for b in range(13 if not lod else 9):
  f=.36+b/(22 if not lod else 16);root=trunk[min(7,int(f*8))];a=b*2.399+v
  radial=Vector((math.cos(a),math.sin(a),.12));extent=(1-f)*3.4+.7
  end=root+radial*extent+Vector((.2,0,.4));g.tube(root,end,.06*(1-f)+.015,wood,.15,6,4)
  for j in range(6 if not lod else 4):
   angle=a+random.uniform(-1.15,1.15);p=end+Vector((math.cos(angle),math.sin(angle),random.uniform(-.3,.3)))*random.uniform(.05,.8)
   for k in range(32 if not lod else 16):
    an=angle+k*2.399;axis=Vector((math.cos(an),math.sin(an),random.uniform(-.25,.8)))
    g.leaf(p+axis*random.uniform(.08,.35),axis,random.uniform(.35,.65),.05,C(['486B50','5D7C51','708550'][k%3]))
 return g.object('Pin_Laricio_'+str(v)+('_Low' if lod else '_High'))
for v in range(3):
 for lod in (0,1):stats.append(export(pine(v,lod)))
# Weathered granite forms: segmented lobes form offset ledges and deep vertical joints.
# Sculpted rounded granite with asymmetric shoulders and vertical erosion gullies.
def rock(v,tower=False,pebble=False):
 random.seed(700+v+(100 if tower else 0));g=Geo()
 lobes=4 if tower else 3
 for l in range(lobes):
  H=random.uniform(2.5,3.8) if tower else random.uniform(.85,1.3)
  if tower and l>0:H*=random.uniform(.47,.83)
  cx=(l-(lobes-1)/2)*(.45 if tower else .38)
  cy=random.uniform(-.24,.24);radius=random.uniform(.64,.90) if tower else random.uniform(.57,.75)
  bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=(1 if pebble else 3),radius=1)
  temp=bpy.context.object
  pts=[]
  for ve in temp.data.vertices:
   d=ve.co.normalized();f=(d.z+1)*.5;a=math.atan2(d.y,d.x)
   irregular=1+.085*math.sin(d.x*6+l)+.055*math.sin(d.y*8+d.z*4+v)
   gully=1-.36*max(0,math.cos(a*3+l+.28*math.sin(f*7)))**16
   shoulder=(1-.19*f) if tower else 1
   xx=d.x*radius*irregular*gully*shoulder
   xx=min(xx,radius*(.72-.21*f+.08*math.sin(f*9+l)))
   xx=max(xx,-radius*(.78-.10*f))
   x=cx+xx+f*.28*math.sin(v+l)+(.055 if f>.52 else 0)
   y=cy+min(d.y*radius*.87*irregular*gully,radius*(.68-.13*f))
   z=H*f+(.18*math.sin(d.x*5+d.y*4+v))*math.sin(f*math.pi)
   pts.append(Vector((x,y,z)))
  for poly in temp.data.polygons:
   z=sum(pts[i].z for i in poly.vertices)/len(poly.vertices);paint=.92+.12*math.sin(z*5+cx*3)
   col=tuple(c*paint if n<3 else c for n,c in enumerate(C(['D6B3A1','C6AD98','DCC3A5','BBA392'][(l+v)%4])))
   ps=[pts[i] for i in poly.vertices];g.face(ps,col,0,[(p.x,p.z) for p in ps])
  bpy.data.objects.remove(temp,do_unlink=True)
 ob=g.object(('Galet' if pebble else 'Aiguille' if tower else 'Granit')+'_'+str(v),True)
 for p in ob.data.polygons:p.use_smooth=pebble or p.normal.z>.65
 return ob

for v in range(5):stats.append(export(rock(v)))
for v in range(7):stats.append(export(rock(v,True)))
stats.append(export(rock(0,False,True)))
# Keep the entire original editable mesh kit in a Blender source file.
for ob in bpy.data.objects:ob.hide_set(False)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT,'CorseVR_Corsican_Kit.blend'))
with open(os.path.join(ROOT,'kit_stats.json'),'w') as f:json.dump(stats,f,indent=2)
print('CORSEVR_KIT_COMPLETE',len(stats))

