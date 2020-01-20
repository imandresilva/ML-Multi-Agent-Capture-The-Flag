using System;

public class SeenBits {

    private char[] bits;
		
    public SeenBits(int length) {
        bits = new char[length];
        for (int i = 0; i < length; i++)
            bits[i] = '0';
    }

    public void Update(Tuple<ObjectType,float> seenObject, int eyeIt, AttackingAgent b) {
        ObjectType t = seenObject.Item1;
        if (seenObject.Item2 > 1) {
            switch (t) {
                case ObjectType.Attacker:
                case ObjectType.Defender:
                    if (b.isAlly(t))
                        bits[3 * 3 + eyeIt] = '1';
                    else
                        bits[4 * 3 + eyeIt] = '1';
                    break;
                default:
                    bits[(int)t * 3 + eyeIt] = '1';
                    break;
					
            }
        } else {
            switch (t) {
                case ObjectType.Attacker:
                case ObjectType.Defender: {
                    if (!b.isAlly(t)) {
                        bits[6 * 3 + eyeIt] = '1';
                    }
                    break;
                }

                case ObjectType.Wall:
                    bits[eyeIt] = '1';
                    bits[5 * 3 + eyeIt] = '1';
                    break;
            }
        }
			
    }
		
    public override string ToString() {
        return new string(bits);
    }
}