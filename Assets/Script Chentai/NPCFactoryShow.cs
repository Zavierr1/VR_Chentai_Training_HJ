using System.Collections;
using UnityEngine;
using UnityEngine.Events; // >>> TAMBAHAN: Wajib untuk sistem Event UI

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

    // >>> TAMBAHAN BARU: Event untuk memicu UI Pop-Out
    [Header("Event UI & Interaksi")]
    [Tooltip("Apa yang terjadi setelah NPC selesai memencet tombol? (Misal: Munculkan UI Pop Out)")]
    public UnityEvent onNPCSelesaiMencetTombol;

    void Start()
    {
        if (autoStartOnPlay)
            MesinSelesaiDiperbaiki();
    }

    public void MesinSelesaiDiperbaiki()
    {
        StartCoroutine(SequenceKerjaProfesional());
    }

    private IEnumerator SequenceKerjaProfesional()
    {
        yield return new WaitForSeconds(0.1f); 
        enableFootIK = true;

        Quaternion rotasiAwalStandby = transform.rotation;

        // 1. JALAN KE MESIN
        Debug.Log("[NPC] 1. Berangkat ke mesin...");
        anim.ResetTrigger("Idle"); 
        anim.SetTrigger("Walk");
        yield return StartCoroutine(JalanMaju(durasiJalanKeMesin));

        Quaternion rotasiDiMesin = transform.rotation;

        // 2. PENCET TOMBOL
        Debug.Log("[NPC] 2. Menyalakan mesin (Push Button).");
        anim.ResetTrigger("Walk");
        anim.SetTrigger("PushButton");
        yield return new WaitForSeconds(durasiPencetTombol);

        // >>> PANGGIL EVENT POP-OUT UI DI SINI <<<
        // Tepat setelah NPC selesai memencet tombol, dia akan memberi sinyal untuk memunculkan layar UI!
        onNPCSelesaiMencetTombol?.Invoke();

        // 3. MENGHADAP PEMAIN (KAMERA)
        Debug.Log("[NPC] 3. Nengok ke arah kamera/pemain...");
        if (targetPemain == null && Camera.main != null) 
            targetPemain = Camera.main.transform; 
            
        if (targetPemain != null)
        {
            yield return StartCoroutine(PutarMenghadap(targetPemain));
        }

        // 4. THUMBS UP
        Debug.Log("[NPC] 4. Mesin aman, Thumbs Up!");
        anim.ResetTrigger("PushButton");
        anim.SetTrigger("Thumbs");
        yield return new WaitForSeconds(durasiThumbs);

        // 5. PUTAR BADAN BALIK UNTUK PULANG
        Debug.Log("[NPC] 5. Balik badan untuk pulang...");
        Quaternion rotasiPulang = rotasiDiMesin * Quaternion.Euler(0, 180f, 0);
        yield return StartCoroutine(PutarKeRotasi(rotasiPulang));

        // 6. JALAN KEMBALI
        Debug.Log("[NPC] 6. Jalan kembali ke pos...");
        anim.ResetTrigger("Thumbs");
        anim.SetTrigger("Walk");
        yield return StartCoroutine(JalanMaju(durasiJalanKembali));

        // 7. MENGHADAP CONVEYOR & IDLE
        Debug.Log("[NPC] 7. Posisi stand by menghadap conveyor.");
        anim.ResetTrigger("Walk");
        anim.SetTrigger("Idle");
        
        Quaternion rotasiAkhir = rotasiAwalStandby * Quaternion.Euler(0, sudutMenghadapConveyor, 0);
        yield return StartCoroutine(PutarKeRotasi(rotasiAkhir));
    }

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

    private IEnumerator PutarKeRotasi(Quaternion targetRotasi)
    {
        while (Quaternion.Angle(transform.rotation, targetRotasi) > 0.5f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotasi, turnSpeed * Time.deltaTime);
            yield return null;
        }
        transform.rotation = targetRotasi; 
    }

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