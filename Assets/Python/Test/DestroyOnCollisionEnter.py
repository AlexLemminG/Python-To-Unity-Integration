from UnityEngine import Rigidbody, Color,Renderer, Random, Mathf, Time

collisionsCount = 0
def Update():
	global output
	global collisionsCount
	output = owner.GetComponent[Rigidbody]().velocity
	gameObject.GetComponentInChildren[Renderer]().material.color = Color.HSVToRGB(Mathf.Repeat((Time.time + collisionsCount * 0.5)*0.1, 1), 1,1)

def OnCollisionEnter(col):
	global collisionsCount
	collisionsCount += 1