from UnityEngine import Vector3, Color, Renderer
import PythonBehaviour
import UnityEngine

Time = UnityEngine.Time()
Mathf = UnityEngine.Mathf()
lastT = int(Time.time*10)

def Awake():
	global copied
	copied = False
	global iterations
	iterations = 10
def OnEnable():
	transform.position += Vector3.up

def OnDisable():
	transform.position -= Vector3.up

def Update():
	global lastT
	global copied
	global iterations
	transform.Rotate(0, UnityEngine.Mathf.Sin(Time.time)*180*Time.deltaTime*4,0)
	rot = transform.localEulerAngles
	rot[2] = 0
	transform.localEulerAngles = rot

	# pos = transform.position
	# pos[1] = Mathf.Sin(Time.time * 3) * float(1.6)
	# transform.position = pos
	currentT = int(Time.time*10)
	if not currentT == lastT and not copied and iterations > 0:
		lastT = currentT
		copied = True
		clone = UnityEngine.GameObject.Instantiate(gameObject, transform.position + Vector3(1,0,0), UnityEngine.Quaternion.identity)
		clone.name = gameObject.name
		clone.GetComponent[Renderer]().material.color = Color.HSVToRGB(Mathf.Repeat((clone.transform.position.x - iterations) /float(16.0), 1.0), 1, 1)
		clone.GetComponent[PythonBehaviour]().scope.iterations = iterations-2