using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoilStacker : MonoBehaviour
{
    [Header("Pengaturan Tumpukan")]
    public string foilTag = "Foil";
    public int targetStack = 10;
    public float jarakTumpukan = 0.03f; 

    [Header("Waktu & Delay")]
    public float delaySebelumJalan = 0.5f;
    public float cooldownArea = 1.5f;

    private List<Transform> tumpukanFoil = new List<Transform>();
    private bool sedangDilepas = false;

    // Menyimpan posisi fondasi dari foil pertama
    private Vector3 posisiDasarTumpukan;

    private void OnTriggerEnter(Collider other)
    {
        if (sedangDilepas) return;

        if (other.transform.parent != null)
        {
            Transform parentUtama = other.transform.parent;

            if (parentUtama.CompareTag(foilTag) && !tumpukanFoil.Contains(parentUtama))
            {
                Rigidbody[] allRbs = parentUtama.GetComponentsInChildren<Rigidbody>();
                foreach (Rigidbody rb in allRbs)
                {
                    rb.isKinematic = true;
                }

                // ---------------- LOGIKA POSISI BARU (ANTI TELEPORT) ----------------
                if (tumpukanFoil.Count == 0)
                {
                    // Foil PERTAMA masuk: 
                    // Ambil tinggi (Y) asli foil tersebut karena dia masih menempel di conveyor.
                    // Tarik X dan Z ke tengah kotak merah agar tumpukannya lurus memusat.
                    posisiDasarTumpukan = new Vector3(transform.position.x, parentUtama.position.y, transform.position.z);
                }

                // Susun posisinya ke atas berdasarkan posisi foil pertama
                Vector3 posisiTumpuk = posisiDasarTumpukan + (transform.up * (tumpukanFoil.Count * jarakTumpukan));
                
                parentUtama.position = posisiTumpuk;
                
                // Samakan rotasinya dengan rotasi kotak merah agar semua lurus rapi
                parentUtama.rotation = transform.rotation; 
                // --------------------------------------------------------------------

                tumpukanFoil.Add(parentUtama);

                if (tumpukanFoil.Count > 1)
                {
                    parentUtama.SetParent(tumpukanFoil[0]);
                }

                if (tumpukanFoil.Count >= targetStack)
                {
                    StartCoroutine(LepaskanTumpukan());
                }
            }
        }
    }

    private IEnumerator LepaskanTumpukan()
    {
        sedangDilepas = true;

        yield return new WaitForSeconds(delaySebelumJalan);

        for (int i = 1; i < tumpukanFoil.Count; i++)
        {
            if (tumpukanFoil[i] != null) tumpukanFoil[i].SetParent(null);
        }

        foreach (Transform parentUtama in tumpukanFoil)
        {
            if (parentUtama != null)
            {
                Rigidbody[] allRbs = parentUtama.GetComponentsInChildren<Rigidbody>();
                foreach (Rigidbody rb in allRbs)
                {
                    rb.isKinematic = false;
                }
            }
        }

        tumpukanFoil.Clear();
        yield return new WaitForSeconds(cooldownArea);
        sedangDilepas = false;
    }
}