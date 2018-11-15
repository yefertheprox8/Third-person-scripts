using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Automovil : MonoBehaviour {
    WheelCollider[] m_whellColliders;
    Transform[] m_couchosMallas;
    Rigidbody rigi;

    [SerializeField] Transform CenterMass;

    float frictionForwa;
    float frictionSide;

    [HideInInspector]
    public bool encendido;

    bool frenar = false;
    bool cambiando = false;

    WheelFrictionCurve RuedasTracerasFriccion;

    [SerializeField] private AudioClip sound_AH;
    [SerializeField] private AudioClip sound_AL;
    [SerializeField] private AudioClip sound_DH;
    [SerializeField] private AudioClip sound_DL;
    [SerializeField] private AudioClip sound_S;

    private AudioSource audioS;

    [SerializeField] private int v;
    private int[] velocidadPunta = new int[5] { 200, 600, 800, 1000, 1100 };

    private int MaxVelocity;

    WheelFrictionCurve curva, curva2;
    float stiffnessBackWheels, stiffnessBackWheels2;
    // Use this for initialization
    void Start () {
        audioS = GetComponent<AudioSource>();
        rigi = GetComponent<Rigidbody>();

        m_whellColliders = new WheelCollider[4];
        m_couchosMallas = new Transform[4];

        for (int i = 0; i < m_whellColliders.Length; i++)
        {
            m_whellColliders[i] = transform.GetChild(i + 1).GetComponent<WheelCollider>();
        }

        for (int i = 0; i < m_couchosMallas.Length; i++)
        {
            m_couchosMallas[i] = transform.GetChild(i + 1).GetChild(0);
        }

        rigi.centerOfMass = CenterMass.localPosition;
        curva = m_whellColliders[2].sidewaysFriction;
        curva2 = m_whellColliders[1].sidewaysFriction;
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        for (int i = 0; i < m_whellColliders.Length; i++)
        {
            Vector3 pos;
            Quaternion rot;

            m_whellColliders[i].GetWorldPose(out pos, out rot);

            m_couchosMallas[i].position = pos;
            m_couchosMallas[i].rotation = rot;
        }

        if(Input.GetKeyDown(KeyCode.P))
            encendido = !encendido;

        WheelHit hit = new WheelHit();

        float[] volumen = new float[4];

        for (int i = 0; i < m_whellColliders.Length; i++)
        {
            m_whellColliders[i].GetGroundHit(out hit);

            if(Mathf.Abs(hit.forwardSlip) >= .8f || Mathf.Abs(hit.sidewaysSlip) >= .3f)
            {

                if(m_couchosMallas[i].GetComponent<AudioSource>().volume < .4f)
                    volumen[i] = .4f;

                continue;
            }
            volumen[i] = 0;
            
        }

        float velocidadAudio = (Input.GetAxisRaw("Vertical") >= 0) ? .3f - (m_whellColliders[2].rpm + m_whellColliders[3].rpm) / velocidadPunta[v] : .3f + (m_whellColliders[2].rpm + m_whellColliders[3].rpm) / velocidadPunta[v];

        velocidadAudio = Mathf.Clamp(velocidadAudio, .3f, 3);

        audioS.pitch = Mathf.Lerp(audioS.pitch, velocidadAudio, .1f);

        //Debug.Log(m_whellColliders[2].rpm);

        for (int i = 0; i < m_couchosMallas.Length; i++)
        {
            m_couchosMallas[i].GetComponent<AudioSource>().volume = Mathf.Lerp(m_couchosMallas[i].GetComponent<AudioSource>().volume, volumen[i], .4f);
        }

        
        //print(GetComponent<Rigidbody>().velocity.magnitude);
        //Debug.Log("fDir: " + hit.forwardDir + " fSplit: " + Mathf.Abs(hit.forwardSlip) + " sDir: " + hit.sidewaysDir + " sSplit: " + Mathf.Abs(hit.sidewaysSlip));

        if (encendido)
        {
            if(!cambiando)
                frenar = Input.GetKey(KeyCode.Space);

            if (Input.GetKeyDown(KeyCode.LeftShift))
                StartCoroutine(CambioPalaca(true));

            if (Input.GetKeyDown(KeyCode.LeftControl))
                StartCoroutine(CambioPalaca(false));

            float rotCauchos = 30 / (rigi.velocity.magnitude / 5);

            rotCauchos = Mathf.Clamp(rotCauchos, 5, 30);

            m_whellColliders[0].steerAngle = Input.GetAxis("Horizontal") * rotCauchos;
            m_whellColliders[1].steerAngle = Input.GetAxis("Horizontal") * rotCauchos;


            if (frenar)
            {
                stiffnessBackWheels = .0f;
                stiffnessBackWheels2 = .2f;
                m_whellColliders[2].brakeTorque = m_whellColliders[3].brakeTorque = 8000;

                if (rigi.velocity.magnitude < MaxVelocity)
                    m_whellColliders[0].motorTorque = m_whellColliders[1].motorTorque = Input.GetAxis("Vertical") * -700f;
                else
                    m_whellColliders[0].motorTorque = m_whellColliders[1].motorTorque = Input.GetAxis("Vertical") * 0;

                transform.Rotate(0, Input.GetAxis("Horizontal") * .2f, 0);
            }
            else
            {
                transform.Rotate(0, Input.GetAxis("Horizontal") * .4f, 0);
                stiffnessBackWheels = 3;
                stiffnessBackWheels2 = 3;
                m_whellColliders[2].brakeTorque = m_whellColliders[3].brakeTorque = 0;

                if(rigi.velocity.magnitude < MaxVelocity)
                    m_whellColliders[0].motorTorque = m_whellColliders[1].motorTorque = Input.GetAxis("Vertical") * -700f;
                else
                    m_whellColliders[0].motorTorque = m_whellColliders[1].motorTorque = Input.GetAxis("Vertical") * 0;

                switch (v)
                {
                    case 0:
                        MaxVelocity = 5;
                        break;
                    case 1:
                        MaxVelocity = 15;
                        break;
                    case 2:
                        MaxVelocity = 20;
                        break;
                    case 3:
                        MaxVelocity = 25;
                        break;
                    case 4:
                        MaxVelocity = 100;
                        break;
                    default:
                        break;
                }
            }

            if (Input.GetAxisRaw("Horizontal") != 0)
            {
                RuedasTracerasFriccion = m_whellColliders[2].sidewaysFriction;

                //RuedasTracerasFriccion.extremumSlip = Mathf.MoveTowards(m_whellColliders[2].sidewaysFriction.extremumSlip, 2, .4f * Time.deltaTime);

                //m_whellColliders[2].sidewaysFriction = m_whellColliders[3].sidewaysFriction = RuedasTracerasFriccion;
            }
            else
            {
                RuedasTracerasFriccion = m_whellColliders[2].sidewaysFriction;

                //RuedasTracerasFriccion.extremumSlip = Mathf.MoveTowards(m_whellColliders[2].sidewaysFriction.extremumSlip, 1, .8f * Time.deltaTime);

               // m_whellColliders[2].sidewaysFriction = m_whellColliders[3].sidewaysFriction = RuedasTracerasFriccion;
            }
            curva.stiffness = Mathf.Lerp(curva.stiffness, stiffnessBackWheels, (stiffnessBackWheels == 0) ? .25f : (.01f / (rigi.velocity.magnitude / 10)));
            curva.stiffness = (float)System.Math.Round(curva.stiffness, 2);

            curva2.stiffness = Mathf.Lerp(curva2.stiffness, stiffnessBackWheels2, (stiffnessBackWheels2 == 1) ? .25f : (.01f / (rigi.velocity.magnitude / 10)));
            curva2.stiffness = (float)System.Math.Round(curva2.stiffness, 2);
            
            m_whellColliders[0].sidewaysFriction = m_whellColliders[1].sidewaysFriction = curva2;
            m_whellColliders[2].sidewaysFriction = m_whellColliders[3].sidewaysFriction = curva;
        }
        else
        {
            m_whellColliders[2].brakeTorque = m_whellColliders[3].brakeTorque = 15000;
            m_whellColliders[0].motorTorque = m_whellColliders[1].motorTorque = Input.GetAxis("Vertical") * 0;
        }
    }
    IEnumerator CambioPalaca(bool mas)
    {
        cambiando = true;
        frenar = true;

        yield return new WaitForSeconds(.05f);

        frenar = false;
        cambiando = false;

        if (mas)
        {
            v++;
        }
        else
        {
            v--;
        }
    }
}
