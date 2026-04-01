using System.Collections;
using UnityEngine;

public class NPCFactoryShow : MonoBehaviour
{
    [Header("Komponen Utama")]
    public Animator anim;
    
    [Tooltip("Kecepatan dorong badan (Parent) NPC")]
    public float walkSpeed = 1.2f;
    [Tooltip("Kecepatan putar badan NPC")]
    public float turnSpeed = 5.0f;

    [Header("Pengaturan Navigasi (Murni Timer & Sudut)")]
    [Tooltip("Berapa detik NPC jalan lurus ke mesin?")]
    public float durasiJalanKeMesin = 2.0f;
    
    [Tooltip("Berapa derajat NPC harus putar badan setelah dari mesin? (180 = balik badan)")]
    public float sudutPutaranJoget = 180f;
    
    [Tooltip("Berapa detik NPC jalan ke area joget setelah putar badan?")]
    public float durasiJalanKeAreaJoget = 2.0f;

    [Header("Durasi Animasi")]
    public float durasiPencetTombol = 2.0f;
    public float durasiTwerk = 4.0f;
    public float durasiBackflip = 1.5f;
    public float durasiProsesDuduk = 2.0f;

    [Header("Pengaturan IK Kaki (Procedural Animation)")]
    [Tooltip("Nyalakan saat NPC berdiri/jalan agar kaki menapak collider")]
    public bool enableFootIK = false;
    [Tooltip("Layer khusus untuk objek Lantai/Ground")]
    public LayerMask floorLayer;
    [Tooltip("Jarak dari titik Pivot tulang kaki ke bawah telapak sepatu")]
    public float footOffset = 0.12f;
    [Range(0, 1)] public float ikWeight = 1f;

    [Header("Debug")]
    public bool autoStartOnPlay = true;

    void Start()
    {
        if (autoStartOnPlay)
            MesinSelesaiDiperbaiki();
    }

    public void MesinSelesaiDiperbaiki()
    {
        StartCoroutine(SequenceFullPakaiTimer());
    }

    private IEnumerator SequenceFullPakaiTimer()
    {
        // --- OBAT ANTI ERROR LAYER -1 ---
        yield return new WaitForSeconds(0.1f); 

        // NYALAKAN IK SAAT MULAI BERDIRI/JALAN
        enableFootIK = true;

        // 1. JALAN KE MESIN
        Debug.Log("[NPC] 1. Jalan lurus ke mesin...");
        anim.ResetTrigger("Walk");
        anim.SetTrigger("Walk");
        yield return StartCoroutine(JalanMaju(durasiJalanKeMesin));

        // 2. PENCET TOMBOL
        Debug.Log("[NPC] 2. Pencet tombol!");
        anim.ResetTrigger("PushButton");
        anim.SetTrigger("PushButton");
        yield return new WaitForSeconds(durasiPencetTombol);

        // 3. PUTAR BADAN (Cari posisi kosong)
        Debug.Log("[NPC] 3. Putar badan...");
        yield return StartCoroutine(PutarBadan(sudutPutaranJoget));

        // 4. JALAN KE AREA JOGET
        Debug.Log("[NPC] 4. Jalan ke area joget...");
        anim.ResetTrigger("Walk");
        anim.SetTrigger("Walk");
        yield return StartCoroutine(JalanMaju(durasiJalanKeAreaJoget));

        // 5. TWERK
        Debug.Log("[NPC] 5. Twerk!");
        // Matikan IK sejenak jika animasi (seperti twerk/salto) butuh kaki terangkat bebas
        enableFootIK = false; 
        anim.ResetTrigger("Twerk");
        anim.SetTrigger("Twerk");
        yield return new WaitForSeconds(durasiTwerk);

        // 6. BACKFLIP
        Debug.Log("[NPC] 6. Salto!");
        anim.ResetTrigger("Backflip");
        anim.SetTrigger("Backflip");
        yield return new WaitForSeconds(durasiBackflip);

        // Nyalakan IK lagi setelah mendarat
        enableFootIK = true;

        // 7. PERSIAPAN DUDUK
        Debug.Log("[NPC] 7. Duduk di tempat.");
        anim.ResetTrigger("StandToSit");
        anim.SetTrigger("StandToSit");
        yield return new WaitForSeconds(durasiProsesDuduk);

        // 8. MULAI KERJA
        Debug.Log("[NPC] 8. Mulai kerja.");
        anim.ResetTrigger("SitWork");
        anim.SetTrigger("SitWork");
        
        // Matikan IK saat sudah duduk (opsional, tergantung tinggi kursi)
        // enableFootIK = false; 
    }

    // --- FUNGSI MENDORONG PARENT MAJU PAKAI TIMER ---
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

    // --- FUNGSI PUTAR BADAN PAKAI SUDUT ---
    private IEnumerator PutarBadan(float sudutTambahan)
    {
        Quaternion targetRotasi = transform.rotation * Quaternion.Euler(0, sudutTambahan, 0);
        while (Quaternion.Angle(transform.rotation, targetRotasi) > 0.5f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotasi, turnSpeed * Time.deltaTime);
            yield return null;
        }
        transform.rotation = targetRotasi; 
    }

    // --- PROCEDURAL ANIMATION: INVERSE KINEMATICS (IK) UNTUK KAKI ---
    private void OnAnimatorIK(int layerIndex)
    {
        if (anim == null) return;

        if (enableFootIK)
        {
            // Atur bobot IK (1 = posisi di-override full oleh script ini)
            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, ikWeight);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, ikWeight);
            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, ikWeight);
            anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, ikWeight);

            // Proses kaki kiri & kanan
            ProcessFootIK(AvatarIKGoal.LeftFoot);
            ProcessFootIK(AvatarIKGoal.RightFoot);
        }
        else
        {
            // Kembalikan ke animasi bawaan (Forward Kinematics)
            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0);
            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
            anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0);
        }
    }

    private void ProcessFootIK(AvatarIKGoal foot)
    {
        // Ambil posisi kaki bawaan animasi
        Vector3 footPos = anim.GetIKPosition(foot);

        // Tembakkan Raycast dari sedikit di atas kaki ke bawah mencari lantai
        RaycastHit hit;
        if (Physics.Raycast(footPos + Vector3.up * 1.0f, Vector3.down, out hit, 4.0f, floorLayer))
        {
            // 1. Sesuaikan posisi kaki menempel di titik tabrakan collider lantai + offset
            anim.SetIKPosition(foot, hit.point + Vector3.up * footOffset);

            // 2. Sesuaikan rotasi telapak kaki mengikuti kemiringan lantai (opsional tapi bagus)
            Quaternion footRotation = anim.GetIKRotation(foot);
            Quaternion normalRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            anim.SetIKRotation(foot, normalRotation * footRotation);
        }
    }
}