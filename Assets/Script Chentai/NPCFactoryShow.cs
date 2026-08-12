using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Drives an NPC through a scripted factory routine: walk to the machine, push the
// button, face the player, wait for calibration, give a thumbs up, then walk back
// to standby. Includes optional procedural foot IK.
public class NPCFactoryShow : MonoBehaviour
{
    [Header("Komponen Utama")]
    public Animator anim;
    
    [Tooltip("Kecepatan dorong badan (Parent) NPC")]
    public float walkSpeed = 1.2f;
    [Tooltip("Kecepatan putar badan NPC")]
    public float turnSpeed = 5.0f;

    [Header("Referensi Target (Pemain/Kamera)")]
    [Tooltip("Masukkan objek Main Camera atau CCTV ke sini agar NPC menatap ke arah ini saat Thumbs Up")]
    public Transform targetPemain;

    [Header("Pengaturan Navigasi Waktu")]
    [Tooltip("Berapa detik NPC jalan lurus ke mesin?")]
    public float durasiJalanKeMesin = 2.0f;
    [Tooltip("Berapa detik NPC jalan balik ke posisi awal?")]
    public float durasiJalanKembali = 2.0f;
    
    [Header("Pengaturan Rotasi Akhir")]
    [Tooltip("Berapa derajat putaran terakhir agar menghadap conveyor saat idle? (Misal 90 atau -90)")]
    public float sudutMenghadapConveyor = 90f;

    [Header("Durasi Animasi")]
    public float durasiPencetTombol = 2.0f;
    public float durasiThumbs = 2.0f;

    [Header("Pengaturan IK Kaki (Procedural Animation)")]
    public bool enableFootIK = false;
    public LayerMask floorLayer;
    public float footOffset = 0.12f;
    [Range(0, 1)] public float ikWeight = 1f;

    [Header("Debug")]
    public bool autoStartOnPlay = true;

    [Header("Event UI & Interaksi")]
    [Tooltip("Apa yang terjadi setelah NPC selesai memencet tombol? (Misal: Munculkan UI Pop Out)")]
    public UnityEvent onNPCSelesaiMencetTombol;

    [Tooltip("Apa yang terjadi saat NPC sudah sepenuhnya kembali ke posisi awal dan diam (Idle)?")]
    public UnityEvent onNPCKembaliKePosisi;

    // Flag that keeps the NPC paused while the player finishes calibration.
    [HideInInspector] 
    public bool sedangMenungguKalibrasi = false;

    // Auto-starts the sequence on play when enabled.
    void Start()
    {
        if (autoStartOnPlay)
            MesinSelesaiDiperbaiki();
    }

    // Begins the full NPC work sequence.
    public void MesinSelesaiDiperbaiki()
    {
        StartCoroutine(SequenceKerjaProfesional());
    }

    // Resumes the NPC animation after the calibration is complete.
    public void LanjutkanReaksiNPC()
    {
        sedangMenungguKalibrasi = false;
        Debug.Log("[NPC] Menerima sinyal kalibrasi beres. Melanjutkan reaksi...");
    }

    // Runs the NPC through its full scripted routine.
    private IEnumerator SequenceKerjaProfesional()
    {
        yield return new WaitForSeconds(0.1f); 
        enableFootIK = true;

        Quaternion rotasiAwalStandby = transform.rotation;

        // 1. WALK TO THE MACHINE.
        Debug.Log("[NPC] 1. Berangkat ke mesin...");
        anim.ResetTrigger("Idle"); 
        anim.SetTrigger("Walk");
        yield return StartCoroutine(JalanMaju(durasiJalanKeMesin));

        Quaternion rotasiDiMesin = transform.rotation;

        // 2. PRESS THE BUTTON.
        Debug.Log("[NPC] 2. Menyalakan mesin (Push Button).");
        anim.ResetTrigger("Walk");
        anim.SetTrigger("PushButton");
        yield return new WaitForSeconds(durasiPencetTombol);

        // Trigger the event that shows the calibration UI panel.
        onNPCSelesaiMencetTombol?.Invoke();

        // 3. FACE THE PLAYER (CAMERA).
        Debug.Log("[NPC] 3. Nengok ke arah kamera/pemain...");
        if (targetPemain == null && Camera.main != null) 
            targetPemain = Camera.main.transform; 
            
        if (targetPemain != null)
        {
            yield return StartCoroutine(PutarMenghadap(targetPemain));
        }

        // 3.5 SET IDLE AND WAIT FOR THE PLAYER.
        Debug.Log("[NPC] 3.5 Menunggu pemain menyelesaikan kalibrasi...");
        anim.ResetTrigger("PushButton");
        anim.SetTrigger("Idle"); // Force the NPC to stand still and watch the player.
        
        sedangMenungguKalibrasi = true;
        // Execution halts here until the flag becomes false.
        yield return new WaitUntil(() => !sedangMenungguKalibrasi); 

        // 4. THUMBS UP.
        Debug.Log("[NPC] 4. Kalibrasi sukses, Thumbs Up!");
        anim.ResetTrigger("Idle"); // Disable idle and switch to the thumbs animation.
        anim.SetTrigger("Thumbs");
        yield return new WaitForSeconds(durasiThumbs);

        // 5. TURN AROUND TO WALK BACK.
        Debug.Log("[NPC] 5. Balik badan untuk pulang...");
        Quaternion rotasiPulang = rotasiDiMesin * Quaternion.Euler(0, 180f, 0);
        yield return StartCoroutine(PutarKeRotasi(rotasiPulang));

        // 6. WALK BACK.
        Debug.Log("[NPC] 6. Jalan kembali ke pos...");
        anim.ResetTrigger("Thumbs");
        anim.SetTrigger("Walk");
        yield return StartCoroutine(JalanMaju(durasiJalanKembali));

        // 7. FACE THE CONVEYOR AND GO IDLE.
        Debug.Log("[NPC] 7. Posisi stand by menghadap conveyor.");
        anim.ResetTrigger("Walk");
        anim.SetTrigger("Idle");
        
        Quaternion rotasiAkhir = rotasiAwalStandby * Quaternion.Euler(0, sudutMenghadapConveyor, 0);
        yield return StartCoroutine(PutarKeRotasi(rotasiAkhir));

        // 8. DONE.
        Debug.Log("[NPC] 8. Sequence selesai. Memicu Event Sukses...");
        onNPCKembaliKePosisi?.Invoke();
    }

    // Rotates the NPC to face the target.
    private IEnumerator PutarMenghadap(Transform target)
    {
        Vector3 arahKeTarget = target.position - transform.position;
        arahKeTarget.y = 0; 
        if (arahKeTarget != Vector3.zero)
        {
            Quaternion targetRotasi = Quaternion.LookRotation(arahKeTarget);
            yield return StartCoroutine(PutarKeRotasi(targetRotasi));
        }
    }

    // Moves the NPC forward for a set duration.
    private IEnumerator JalanMaju(float durasi)
    {
        float timer = 0f;
        while (timer < durasi)
        {
            timer += Time.deltaTime;
            transform.position += transform.forward * walkSpeed * Time.deltaTime;
            yield return null; 
        }
    }

    // Smoothly rotates the NPC to the target rotation.
    private IEnumerator PutarKeRotasi(Quaternion targetRotasi)
    {
        while (Quaternion.Angle(transform.rotation, targetRotasi) > 0.5f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotasi, turnSpeed * Time.deltaTime);
            yield return null;
        }
        transform.rotation = targetRotasi; 
    }

    // Applies procedural foot IK when enabled.
    private void OnAnimatorIK(int layerIndex)
    {
        if (anim == null) return;
        if (enableFootIK)
        {
            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, ikWeight);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, ikWeight);
            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, ikWeight);
            anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, ikWeight);
            ProcessFootIK(AvatarIKGoal.LeftFoot);
            ProcessFootIK(AvatarIKGoal.RightFoot);
        }
        else
        {
            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0);
            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
            anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0);
        }
    }

    // Casts a ray downward to place a foot on the floor.
    private void ProcessFootIK(AvatarIKGoal foot)
    {
        Vector3 footPos = anim.GetIKPosition(foot);
        RaycastHit hit;
        if (Physics.Raycast(footPos + Vector3.up * 1.0f, Vector3.down, out hit, 4.0f, floorLayer))
        {
            anim.SetIKPosition(foot, hit.point + Vector3.up * footOffset);
            Quaternion footRotation = anim.GetIKRotation(foot);
            Quaternion normalRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            anim.SetIKRotation(foot, normalRotation * footRotation);
        }
    }
}
