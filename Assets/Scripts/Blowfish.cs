using UnityEngine;
using System.Collections;

public class Blowfish : Fish
{
    protected override IEnumerator Swim()
    {
        while (true)
        {
            float waitFor = Random.Range(0, 5.0f);
            float swimTime = Random.Range(0.5f, 2.0f);
            float t = 0f;

            Quaternion targetRotation = PickRotation();

            while (t < swimTime)
            {
                if (IsDead || stopSwim)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    StopCoroutine(Swim());
                    yield break;
                }
                waitFor = (target == null) ? Random.Range(0, 5.0f) : 0;

                float multiplyFactor = (target == null) ? 2 : 1;
                animation.Stop("Idle");
                PlayAnimation("Swim");
                t += Time.deltaTime;
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.deltaTime * speed * multiplyFactor));
                rb.linearVelocity = Random.Range(speed / 2, speed * 1.5f) * transform.forward;

                if (rb.position.y > maxY)
                {
                    Vector3 pos = rb.position;
                    pos.y = maxY;
                    rb.position = pos;
                    Vector3 v = rb.linearVelocity;
                    if (v.y > 0)
                    {
                        v.y = 0;
                        rb.linearVelocity = v;
                    }
                }
                yield return new WaitForFixedUpdate();
            }
            animation.Play("Idle");
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Utils.Random(Vector3.zero, rb.linearVelocity / 2);
            yield return new WaitForSeconds(waitFor);
        }
    }
}
