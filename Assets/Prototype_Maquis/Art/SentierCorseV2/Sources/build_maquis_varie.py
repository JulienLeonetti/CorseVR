import os, math, random, json
import bpy
from mathutils import Vector
# Geometry helpers from our first kit, retaining the rounded-leaf implementation.
base=r'C:\Users\jeanj\Documents\Codex\2026-10-22\je-x20\work\sentier_kit\build_sentier_kit.py'
exec(open(base,encoding='utf-8').read().split("for species,n in [('Chene'")[0])
ROOT=os.path.abspath('work/sentier_v2/models');os.makedirs(ROOT,exist_ok=True)
stats=[]

def small_spray(g,start,axis,length,seed,low=False):
 r=random.Random(seed);d=Vector(axis).normalized();side=d.cross(Vector((0,0,1))).normalized()
 g.tube(start,start+d*length,.002,C('79724B'),.01,3,1)
 for j in range(3 if low else 4):
  p=start+d*length*(.18+j*.22)
  for sign in [-1,1]:
   g.leaf(p,(side*sign+d*.25+Vector((0,0,r.uniform(-.1,.35)))),r.uniform(.052,.085)*(1.20 if low else 1),.023,C(['5D793E','6B8647','466C3D'][j%3]))

def lentisque(v,low):
 random.seed(3108+v*407);g=Geo();g.low=True
 spread=[1.10,.88,1.25,1.0][v];H=[.80,1.18,.65,1.03][v]
 for j in range(12 if not low else 7):
  angle=random.uniform(0,math.tau);rad=Vector((math.cos(angle),math.sin(angle),0))
  start=Vector((random.uniform(-.18,.18),random.uniform(-.18,.18),-.05))
  knee=start+rad*spread*.45+Vector((0,0,H*.45))
  end=start+rad*spread*random.uniform(.48,.92)+Vector((0,0,H*random.uniform(.65,1)))
  g.tube(start,knee,.024,C('71654B'),.045,5,2,.7);g.tube(knee,end,.017,C('71654B'),.04,4,2,.15)
  for twig in range(5 if not low else 3):
   a=angle+random.uniform(-1.2,1.2);origin=knee.lerp(end,random.uniform(.25,1))
   tip=origin+Vector((math.cos(a)*.23,math.sin(a)*.23,.15))
   g.tube(origin,tip,.005,C('71654B'),.01,3,1,.2)
   for k in range(3 if not low else 2):
    an=a+random.uniform(-1.7,1.7)
    small_spray(g,origin.lerp(tip,.3+k*.3),Vector((math.cos(an),math.sin(an),random.uniform(.1,.7))),.20+random.random()*.06,10000+j*61+twig*7+k,low)
 # Fill the lower crown as well, avoiding a bouquet of identical bare stems.
 for n in range(160 if low else 580):
  a=random.uniform(0,math.tau);r=math.sqrt(random.random());z=H*(.28+.62*math.sqrt(max(0,1-r*r)))
  p=Vector((math.cos(a)*r*spread*.88,math.sin(a)*r*spread*.75,z+random.uniform(-.11,.11)))
  an=random.uniform(0,math.tau);g.leaf(p,Vector((math.cos(an),math.sin(an),random.uniform(-.15,.6))),.105 if low else .078,.025,C(['577A40','6A864B','4A6E3B'][n%3]))
 return g.object('LentisqueRampant_'+str(v)+('_Low' if low else '_High'))

def arbousier(v,low):
 random.seed(447+v*299);g=Geo();g.low=True;H=[1.65,2.15,1.9][v]
 for j in range(5):
  a=j*2.399+v;rad=Vector((math.cos(a),math.sin(a),0));mid=rad*.25+Vector((0,0,H*.45))
  end=rad*random.uniform(.45,.9)+Vector((.20*v,0,H*random.uniform(.68,1)))
  g.tube(Vector((0,0,-.02)),mid,.042,C('865F45'),.08,5,3,.65);g.tube(mid,end,.027,C('865F45'),.06,5,2,.1)
  for k in range(3):
   theta=a+(k-1)*.8;tip=end+Vector((math.cos(theta)*.28,math.sin(theta)*.28,.05))
   g.tube(mid.lerp(end,.5),tip,.01,C('906343'),.03,4,2)
   canopy(g,tip,Vector((.42,.39,.43)),18 if low else 52,900+j*31+k*7+v*77,'Arbousier',low)
 return g.object('ArbousierBuisson_'+str(v)+('_Low' if low else '_High'))

def ciste(v,low):
 random.seed(99+v*151);g=Geo();g.low=True
 for j in range(10 if not low else 6):
  a=random.uniform(0,math.tau);rad=Vector((math.cos(a),math.sin(a),0));H=[.40,.65,.48,.72][v]
  end=rad*random.uniform(.25,.52)+Vector((0,0,H*random.uniform(.65,1)))
  g.tube(Vector((0,0,-.015)),end,.008,C('91866B'),.04,4,2,.15)
  for k in range(6 if not low else 4):
   t=.25+k*(.12 if not low else .19);p=end*t
   side=Vector((-rad.y,rad.x,.12))
   for sign in [-1,1]:
    g.leaf(p,rad*.45+side*sign,.105 if not low else .14,.018,C(['8F9D77','9BA781','7B8E65'][j%3]))
  if j%3==v%3:g.flower(end,.052)
 return g.object('CisteSauge_'+str(v)+('_Low' if low else '_High'))

def bruyere(v,low):
 random.seed(450+v*317);g=Geo();g.low=True;H=[1.5,2.05,1.75][v]
 for j in range(8 if not low else 5):
  angle=j*2.399+v;rad=Vector((math.cos(angle),math.sin(angle),0))
  end=rad*random.uniform(.24,.50)+Vector((0,0,H*random.uniform(.65,1.0)))
  g.tube(Vector((0,0,-.01)),end,.013,C('79634D'),.055,4,3,.08)
  for k in range(6 if not low else 4):
   t=.25+k*(.13 if not low else .20);start=end*t
   theta=angle+k*2.1;tip=start+Vector((math.cos(theta)*.25,math.sin(theta)*.25,.22))
   g.tube(start,tip,.004,C('79634D'),.02,3,1,.1)
   for n in range(6 if not low else 3):
    p=start.lerp(tip,.18+n*(.145 if not low else .32))
    for sign in [-1,1]:
     g.leaf(p,Vector((math.cos(theta+sign),math.sin(theta+sign),.5)),.065 if not low else .09,.009,C(['496A40','69804A','6B8051'][n%3]))
   if k%3==0:
    for n in range(3):g.flower(tip+Vector((0,0,n*.025)),.012)
 return g.object('BruyereFine_'+str(v)+('_Low' if low else '_High'))

for fn,count in [(lentisque,4),(arbousier,3),(ciste,4),(bruyere,3)]:
 for v in range(count):
  for low in [False,True]:stats.append(export(fn(v,low)))

def pin(v):
 random.seed(813+v*43);g=Geo();g.low=True;H=8+v*.8;wood=C('756A58')
 for j in range(6):
  a=Vector((.12*math.sin(j*.5+v),.12*math.cos(j*.7),H*j/6));b=Vector((.12*math.sin((j+1)*.5+v),.12*math.cos((j+1)*.7),H*(j+1)/6))
  g.tube(a,b,.23*(1-j/8),wood,.01,7,1,(1-(j+1)/8)/(1-j/8))
 for j in range(11):
  a=j*2.399+v;f=.49+j*.037;reach=(1-f)*4+.55;d=Vector((math.cos(a),math.sin(a),0))
  base=Vector((0,0,H*f));tip=base+d*reach+Vector((0,0,.45));g.tube(base,tip,.065*(1-j/15),wood,.08,5,2,.15)
  # Small ragged flattened crowns. Designed for middle-distance silhouettes only.
  for k in range(3):
   p=tip+d*(k-1)*.3+Vector((-d.y,d.x,0))*random.uniform(-.48,.48)
   rings=3;sides=11
   for ring in range(rings):
    for q in range(sides):
     def pt(r,n):
      theta=n/sides*math.tau;radius=[.10,.76,.15][r]*(1+.17*math.sin(n*4.3+j+k));z=[-.24,.02,.40][r]
      return p+Vector((math.cos(theta)*radius,math.sin(theta)*radius*.72,z+.07*math.sin(n*5+j)))
     if ring<2:
      col=C(['4D6D4A','54764D','64814F'][(q+j)%3]);g.face([pt(ring,q),pt(ring,q+1),pt(ring+1,q+1),pt(ring+1,q)],col,1,[(0,0),(1,0),(1,1),(0,1)])
 return g.object('PinFond_'+str(v))
for v in range(3):stats.append(export(pin(v)))
for ob in bpy.data.objects:ob.hide_set(False)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT,'Maquis_Varie_Kit.blend'))
with open(os.path.join(ROOT,'kit_stats.json'),'w') as f:json.dump(stats,f,indent=2)
print('MAQUIS_VARIE_COMPLETE',len(stats),sum(x['triangles'] for x in stats))
