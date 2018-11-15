using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour {

    private Vector3 m_CamaraForward, positionMatchTarget;
    private Quaternion rotationMatchTarget;

    private Transform cam;
    [SerializeField] private Transform target, asientoCar, posHead, salirPos;
    [SerializeField] private float velocity;
    [SerializeField] private GameObject car;

    private NavMeshAgent nav;

    private Animator anim;

    private float lerpAngle;

    private bool yendoToPuerta = false;
    private bool Revisar = false, inPosition, inCar, lerpAsiento, caminandoToCar;

    AnimatorStateInfo inf;

    void Start () {
        cam = Camera.main.transform;
        anim = transform.GetChild(0).GetComponent<Animator>();
        inCar = false;
        nav = GetComponent<NavMeshAgent>();
    }
	
	void Update () {

        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        inf = anim.GetCurrentAnimatorStateInfo(0);

        if (inCar)
        {
            anim.SetFloat("Horizontal", h, .04f, Time.deltaTime);
            asientoCar.GetChild(0).localEulerAngles = new Vector3(asientoCar.GetChild(0).localEulerAngles.x, asientoCar.GetChild(0).localEulerAngles.y, h * 50);

            if (Input.GetKeyDown(KeyCode.F))
            {
                car.GetComponent<Automovil>().encendido = false;
                car.GetComponent<Rigidbody>().drag = 2;
                anim.SetBool("Entrar", false);
                StartCoroutine("SalirDelAuto");
            }
        }
        else
        {
            m_CamaraForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;

            Vector3 move = -v * m_CamaraForward + -h * cam.right;
            Vector3 move2 = v * m_CamaraForward + h * cam.right;

            if (caminandoToCar)
            {
                nav.SetDestination(target.position);
                if (nav.remainingDistance < nav.stoppingDistance)
                {
                    caminandoToCar = false;
                    nav.enabled = false;
                    inPosition = true;
                    //anim.applyRootMotion = true;

                    //transform.rotation = target.rotation;
                    //transform.position = target.position;
                    car.GetComponent<Rigidbody>().drag = 0.1f;

                    anim.SetTrigger("Abrir");
                    StartCoroutine("EntrarEnAuto");
                }
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                nav.enabled = true;
                nav.SetDestination(target.position);
                caminandoToCar = true;
            }

            if (lerpAsiento)
            {
                anim.MatchTarget(positionMatchTarget, rotationMatchTarget, AvatarTarget.Root, new MatchTargetWeightMask(Vector3.one, 1f), .0f);
            }

            if (!inPosition && !caminandoToCar)
            {
                Move(-move, true);
            }
            if (!inPosition && caminandoToCar)
            {
                //GetComponent<Rigidbody>().velocity = anim.velocity;
                Move(nav.desiredVelocity, false);

                if(Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
                {
                    caminandoToCar = false;
                    nav.enabled = false;
                }
            }
        }
    }

    private void Move(Vector3 move, bool r)
    {
        if (move.magnitude > 1f) move.Normalize();

        move = transform.InverseTransformDirection(move);

        float m_TurnAmount = Mathf.Atan2(move.x, move.z);
        float m_ForwardAmount = move.z;

        float turnSpeed = Mathf.Lerp(180, 360, m_ForwardAmount * 5);

        transform.Rotate(0, m_TurnAmount * turnSpeed * Time.deltaTime, 0);
        transform.GetChild(0).position = transform.position;
        UpdateAnimation(move, r);
    }

    private void UpdateAnimation(Vector3 move, bool r)
    {
        if (r)
            GetComponent<Rigidbody>().velocity = (anim.deltaPosition) / Time.deltaTime;
        else
            GetComponent<Rigidbody>().velocity = Vector3.zero;

        anim.SetFloat("Vertical", Input.GetKey(KeyCode.LeftShift) ? (move.z) * 2 : (move.z), .1f, Time.deltaTime);
        anim.SetFloat("Horizontal", Input.GetKey(KeyCode.LeftShift) ? (move.x) * 2 : (move.x), .1f, Time.deltaTime);
    }

    IEnumerator EntrarEnAuto()
    {
        var PosFuera = target;

        GetComponent<Rigidbody>().velocity = Vector3.zero;

        while (Vector3.Distance(transform.position, PosFuera.position) < 1f)
        {
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            transform.position = Vector3.MoveTowards(transform.position, PosFuera.position, .05f);
            transform.rotation = Quaternion.Lerp(transform.rotation, PosFuera.rotation, .5f);
            if (Vector3.Distance(transform.position, PosFuera.position) < .04f && Vector3.Distance(transform.GetChild(0).position, PosFuera.position) < .04f)
            {
                break;
            }
            yield return null;
        }

        if (inPosition)
        {
            DesctiveComponent();

            if (!car.GetComponent<Animator>().GetBool("Abrir"))
            {
                while (true)
                {
                    yield return null;
                    if (inf.IsName("Open"))
                    {
                        anim.SetBool("Entrar", true);
                        break;
                    }
                }
                while (inf.IsName("Open"))
                {
                    float tiempo = inf.normalizedTime;

                    if (tiempo > .4f)
                    {
                        if (!car.GetComponent<Animator>().GetBool("Abrir"))
                        {
                            car.GetComponent<Animator>().SetBool("Abrir", true);
                            car.GetComponent<Animator>().SetBool("Cerrar", false);
                        }
                    }
                    if (tiempo > .9f)
                    {
                        break;
                    }
                    yield return null;
                }
            }

            while (true)
            {
                yield return null;
                if (inf.IsName("Entrar"))
                {
                    break;
                }
            }
            anim.applyRootMotion = true;
            while (inf.IsName("Entrar"))
            {
                float tiempo = inf.normalizedTime;

                if (tiempo > .23f)
                {
                    positionMatchTarget = asientoCar.position;
                    rotationMatchTarget = asientoCar.rotation;
                    lerpAsiento = true;
                }
                if (tiempo > .8f)
                {
                    transform.parent = asientoCar;
                }
                if (tiempo > .9f)
                {
                    break;
                }
                yield return null;
            }

            while (true)
            {
                yield return null;
                if (inf.IsName("CerrarPuerta"))
                {
                    break;
                }
            }

            while (inf.IsName("CerrarPuerta"))
            {
                float tiempo = inf.normalizedTime;
                if (tiempo > .5f)
                {
                    car.GetComponent<Animator>().SetBool("Cerrar", true);
                    car.GetComponent<Animator>().SetBool("Abrir", false);
                    break;
                }
                yield return null;
            }

            car.GetComponent<Automovil>().encendido = true;

            inCar = true;
            lerpAsiento = false;
            anim.applyRootMotion = false;
        }
    }

    IEnumerator SalirDelAuto()
    {
        var PosFuera = target;
        while (true)
        {
            yield return null;
            if (inf.IsName("SalirAuto"))
            {
                break;
            }
        }
        anim.applyRootMotion = true;
        while (inf.IsName("SalirAuto"))
        {
            float tiempo = inf.normalizedTime;
            if (tiempo > .02f)
            {
                car.GetComponent<Animator>().SetBool("Abrir", true);
                car.GetComponent<Animator>().SetBool("Cerrar", false);
                positionMatchTarget = transform.position;
                rotationMatchTarget = transform.rotation;
                lerpAsiento = true;
            }

            if(tiempo > .4f)
            {
                transform.position = Vector3.MoveTowards(transform.position, salirPos.position, .01f);
            }

            if (tiempo > .8f)
            {
                transform.parent = null;
            }
            if (tiempo > .9f)
            {
                break;
            }
            yield return null;
        }


        while (true)
        {
            yield return null;
            if (inf.IsName("CerrarPuertaFuera"))
            {
                break;
            }
        }
        anim.applyRootMotion = false;
        while (inf.IsName("CerrarPuertaFuera"))
        {
            float tiempo = inf.normalizedTime;
            transform.GetChild(0).localPosition = Vector3.Lerp(transform.GetChild(0).localPosition, Vector3.zero, 0.1f);
            if (tiempo > .32f)
            {
                car.GetComponent<Animator>().SetBool("Abrir", false);
                car.GetComponent<Animator>().SetBool("Cerrar", true);
                car.GetComponent<Animator>().speed = 2;
            }
            if (tiempo > .9f)
            {
                break;
            }
            yield return null;
        }

        car.GetComponent<Animator>().speed = 1;

        ActiveComponent();
        inCar = false;
        lerpAsiento = false;
        inPosition = false;
    }

    void DesctiveComponent()
    {
        transform.GetChild(0).GetComponent<CapsuleCollider>().enabled = false;
        Destroy(GetComponent<Rigidbody>());
        //nav.enabled = false;
    }

    void ActiveComponent()
    {
        transform.GetChild(0).GetComponent<CapsuleCollider>().enabled = true;
        gameObject.AddComponent<Rigidbody>();
        GetComponent<Rigidbody>().freezeRotation = true;
        //nav.enabled = true;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        anim.SetLookAtPosition(posHead.position);
        anim.SetLookAtWeight(.4f);
        if (inf.IsName("InCar"))
        {
            anim.SetIKPosition(AvatarIKGoal.LeftHand, asientoCar.GetChild(0).GetChild(0).position);
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);

            anim.SetIKRotation(AvatarIKGoal.LeftHand, asientoCar.GetChild(0).GetChild(0).rotation);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
        }

    }
}
