using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TransitionState : MonoBehaviour
{

	private int isOpenHash = Animator.StringToHash("IsOpen");
	// Use this for initialization
	private Animator animator;
	void Start()
	{
		animator = GetComponent<Animator>();
	}

	public void Open()
	{
		animator.SetBool(isOpenHash, true);
	}

	public void Close()
	{
		animator.SetBool(isOpenHash, false);
	}
}

