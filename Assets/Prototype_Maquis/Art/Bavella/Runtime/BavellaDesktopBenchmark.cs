using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace CorseVR{
public class BavellaDesktopBenchmark:MonoBehaviour{
 public Camera targetCamera;public string outputPath;public Vector3[] positions;public Quaternion[] rotations;public string[] labels;
 public int measuredFrames=180,warmupFrames=45;
 [Serializable]public class Sample{public string view;public int frames;public float meanMs,p50Ms,p95Ms,maxMs,averageFps;public float gpuMeanMs;public int gpuSamples,drawCalls,triangles,vertices;public string screen;}
 [Serializable]public class Report{public string unity,gpu,cpu,api;public int ramMB,width,height,msaa;public float renderScale;public bool editor=true;public string context;public List<Sample> views=new List<Sample>();}
 Report report;List<float> values=new List<float>();int stage,frame,gpuCount;double gpuSum;Vector3 oldp;Quaternion oldr;bool finished;
 void Start(){oldp=targetCamera.transform.position;oldr=targetCamera.transform.rotation;var p=UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;report=new Report{unity=Application.unityVersion,gpu=SystemInfo.graphicsDeviceName,cpu=SystemInfo.processorType,api=SystemInfo.graphicsDeviceType.ToString(),ramMB=SystemInfo.systemMemorySize,width=targetCamera.pixelWidth,height=targetCamera.pixelHeight,msaa=p.msaaSampleCount,renderScale=p.renderScale,context="Unity Editor Play mode, Game View fixed HD, desktop mono, no VSync, application target FPS unchanged. Tool/editor activity may affect these timings. Not a WebXR or headset benchmark."};Place();}
 void Place(){targetCamera.transform.SetPositionAndRotation(positions[stage],rotations[stage]);}
 void LateUpdate(){if(finished||report==null)return;Place();frame++;FrameTimingManager.CaptureFrameTimings();if(frame<=warmupFrames)return;values.Add(Time.unscaledDeltaTime*1000);var timing=new FrameTiming[1];if(FrameTimingManager.GetLatestTimings(1,timing)>0&&timing[0].gpuFrameTime>0){gpuSum+=timing[0].gpuFrameTime;gpuCount++;}
 if(values.Count<measuredFrames)return;values.Sort();float sum=0;foreach(var v in values)sum+=v;var s=new Sample{view=labels[stage],frames=values.Count,meanMs=sum/values.Count,p50Ms=values[values.Count/2],p95Ms=values[Mathf.Min(values.Count-1,Mathf.FloorToInt(values.Count*.95f))],maxMs=values[values.Count-1],averageFps=1000/(sum/values.Count),gpuSamples=gpuCount,gpuMeanMs=gpuCount==0?0:(float)(gpuSum/gpuCount)};
 #if UNITY_EDITOR
 s.drawCalls=UnityStats.drawCalls;s.triangles=UnityStats.triangles;s.vertices=UnityStats.vertices;s.screen=UnityStats.screenRes;
 #endif
 report.views.Add(s);stage++;values.Clear();gpuSum=0;gpuCount=0;frame=0;
 if(stage<positions.Length){Place();return;}finished=true;targetCamera.transform.SetPositionAndRotation(oldp,oldr);File.WriteAllText(outputPath,JsonUtility.ToJson(report,true));enabled=false;
 }
 void OnDisable(){if(targetCamera!=null&&report!=null)targetCamera.transform.SetPositionAndRotation(oldp,oldr);}
}}

