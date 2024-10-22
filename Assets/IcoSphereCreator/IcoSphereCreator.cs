using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public static class IcoSphereCreator {
	/*
	level		vertex		triangle			ratio（最开始的一个三角形，被分割成小三角的数量）
	1			24			8					x1
	2			96	 		32					x4
	3			216			72					x9
	4 			384			128					x16
	5			600			200					x25
	6											x36

	所以
		1. level 表示一个三角形的一个边，被分割的线段数量
		2. ratio 与 level 的公式： ratio = level ^ 2; 这个是根据公式推导出来的，不是根据规律推到的
			一个三角形被分割成小的三角形，假设数量Sn, n就是level
			an = n + (n - 1) = 2n - 1;(自己画画三角形，自己分割一下就能找到规律)
			Sn = a1 + a2 + a3 + ... + an
			这个时候就能把an拆解成 an = bn - 1; bn = 2n；(等比数列呼之欲出)
			Xn = b1 + b2 + b3 + ... + bn = 2 * (1 + 2 + 3 + ... n) = n^2 + n;
			Sn = Xn - n = n ^ 2;

	注意
		1. 虽然逻辑上是对一个三角面片进行分割，但实际上，因为是向量的插值（直径不变），所以被分割的三角面片会 “隆起”
		2. 构建出来的mesh, 其顶点数量并没有做优化，比如八面体，它有八个面，但是只需要6个顶点就可以了，但是构建的时候却用了24个顶点，因为相邻的三角形并没有共用顶点。
			这样有坏处，也有好处。坏处就是内存占用较大；但是好处就是，构建出来的mesh是可以更具面片打散的，可以自定义出其他”奇幻“的效果。
	*/

    public static Mesh Create(int level, float radius)
    {
        int nlevel = level * 4;
        int vertexNum = (nlevel * nlevel / 16) * 24; // "nlevel * nlevel / 16" 其实就是一个面被分解成的小三角形的数量。按照上面的推到，可以被简化成 "level * level"

        Vector3[] vertices = new Vector3[vertexNum];
        int[] triangles = new int[vertexNum];
        Vector2[] uv = new Vector2[vertexNum];
		Color[] colors = new Color[vertexNum];

        Quaternion[] init_vectors = new Quaternion[24];
        // 0
        init_vectors[0] = new Quaternion(0, 1, 0, 0);   //the triangle vertical to (1,1,1) 		
        init_vectors[1] = new Quaternion(0, 0, 1, 0);
        init_vectors[2] = new Quaternion(1, 0, 0, 0);
        // 1
        init_vectors[3] = new Quaternion(0, -1, 0, 0);  //to (1,-1,1)
        init_vectors[4] = new Quaternion(1, 0, 0, 0);
        init_vectors[5] = new Quaternion(0, 0, 1, 0);
        // 2
        init_vectors[6] = new Quaternion(0, 1, 0, 0);   //to (-1,1,1)
        init_vectors[7] = new Quaternion(-1, 0, 0, 0);
        init_vectors[8] = new Quaternion(0, 0, 1, 0);
        // 3
        init_vectors[9] = new Quaternion(0, -1, 0, 0);  //to (-1,-1,1)
        init_vectors[10] = new Quaternion(0, 0, 1, 0);
        init_vectors[11] = new Quaternion(-1, 0, 0, 0);
        // 4
        init_vectors[12] = new Quaternion(0, 1, 0, 0);  //to (1,1,-1)
        init_vectors[13] = new Quaternion(1, 0, 0, 0);
        init_vectors[14] = new Quaternion(0, 0, -1, 0);
        // 5
        init_vectors[15] = new Quaternion(0, 1, 0, 0); //to (-1,1,-1)
        init_vectors[16] = new Quaternion(0, 0, -1, 0);
        init_vectors[17] = new Quaternion(-1, 0, 0, 0);
        // 6
        init_vectors[18] = new Quaternion(0, -1, 0, 0); //to (-1,-1,-1)
        init_vectors[19] = new Quaternion(-1, 0, 0, 0);
        init_vectors[20] = new Quaternion(0, 0, -1, 0);
        // 7
        init_vectors[21] = new Quaternion(0, -1, 0, 0);  //to (1,-1,-1)
        init_vectors[22] = new Quaternion(0, 0, -1, 0);
        init_vectors[23] = new Quaternion(1, 0, 0, 0);
        
        int j = 0;  //index on vectors[]

        for (int i = 0; i < 24; i += 3)
        {
            /*
			 *                   c _________d
			 *    ^ /\           /\        /
			 *   / /  \         /  \      /
			 *  p /    \       /    \    /
			 *   /      \     /      \  /
			 *  /________\   /________\/
			 *     q->       a         b
			 */
            for (int p = 0; p < level; p++)
            {   
                //edge index 1
                Quaternion edge_p1 = Quaternion.Lerp(init_vectors[i], init_vectors[i + 2], (float)p / level);				// y -> x
                Quaternion edge_p2 = Quaternion.Lerp(init_vectors[i + 1], init_vectors[i + 2], (float)p / level);			// z -> x
                Quaternion edge_p3 = Quaternion.Lerp(init_vectors[i], init_vectors[i + 2], (float)(p + 1) / level);			// y -> x
                Quaternion edge_p4 = Quaternion.Lerp(init_vectors[i + 1], init_vectors[i + 2], (float)(p + 1) / level);		// z -> x

                for (int q = 0; q < (level - p); q++)
                {   
                    //edge index 2
                    Quaternion a = Quaternion.Lerp(edge_p1, edge_p2, (float)q / (level - p));
                    Quaternion b = Quaternion.Lerp(edge_p1, edge_p2, (float)(q + 1) / (level - p));
                    Quaternion c, d;

                    if(edge_p3 == edge_p4)
                    {
                        c = edge_p3;
                        d = edge_p3;
                    }else
                    {
                        c = Quaternion.Lerp(edge_p3, edge_p4, (float)q / (level - p - 1));
                        d = Quaternion.Lerp(edge_p3, edge_p4, (float)(q + 1) / (level - p - 1));
                    }

                    triangles[j] = j;
                    vertices[j++] = new Vector3(a.x, a.y, a.z);
                    triangles[j] = j;
                    vertices[j++] = new Vector3(b.x, b.y, b.z);
                    triangles[j] = j;
                    vertices[j++] = new Vector3(c.x, c.y, c.z);
					
                    if (q < level - p - 1)		// 什么情况下会有这步逻辑？答: 三角形是按照从下往上，从左往右被分割的；越往下，越往左（a,b,c,d是一个四边形，而不是三角形的时候），这步就会被调用
                    {
                        triangles[j] = j;
                        vertices[j++] = new Vector3(c.x, c.y, c.z);
                        triangles[j] = j;
                        vertices[j++] = new Vector3(b.x, b.y, b.z);
                        triangles[j] = j;
                        vertices[j++] = new Vector3(d.x, d.y, d.z);
                    }
                }
            }
        }
        Mesh mesh = new Mesh();
        mesh.name = "IcoSphere";

        CreateUV(level, vertices, uv);
		CreateColor(level, vertices, colors);

        for (int i = 0; i < vertexNum; i++)
        {
            vertices[i] *= radius;
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
		mesh.colors = colors;
        mesh.RecalculateNormals();
        CreateTangents(mesh);

        return mesh;
    }

    static void CreateUV(int level, Vector3[] vertices, Vector2[] uv)
    {
        int tri = level * level;        // devided triangle count (1,4,9...)
        int uvLimit = tri * 6;  		// range of wrap UV.x 					它包含了第一和第二象限的所有顶点

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = vertices[i];

            Vector2 textureCoordinates;
            if((v.x == 0f)&&(i < uvLimit)) // 第一，第二象限，x == 0 的坐标，让它的 uv == 1 而不是 0；这样才能构成完成的逆时针。
            {
				// 2
                textureCoordinates.x = 1f;
            }
            else
            {
                textureCoordinates.x = Mathf.Atan2(v.x, v.z) / (-2f * Mathf.PI); //注意这里因为有一个-2,所以颠倒了。结合后面的那个x代码，所以uv按照 第二象限 -> 第三象限 -> 第四象限 -> 第一象限 顺序展开 
            }

            if (textureCoordinates.x < 0f)
            {
                textureCoordinates.x += 1f;
            }

            textureCoordinates.y = Mathf.Asin(v.y) / Mathf.PI + 0.5f; // uv按照从上往下展开
            uv[i] = textureCoordinates;
        }

// 		int tt = tri * 3;
// 		Debug.Log($@"[0 * tt + 0]: {uv[0 * tt + 0].x} 
// [1 * tt + 0]: {uv[1 * tt + 0].x} 
// [2 * tt + 0]: {uv[2 * tt + 0].x} 
// [3 * tt + 0]: {uv[3 * tt + 0].x} 
// [4 * tt + 0]: {uv[4 * tt + 0].x} 
// [5 * tt + 0]: {uv[5 * tt + 0].x} 
// [6 * tt + 0]: {uv[6 * tt + 0].x} 
// [7 * tt + 0]: {uv[7 * tt + 0].x}");
		/*
		[0 * tt + 0]: 1 
		[1 * tt + 0]: 1 
		[2 * tt + 0]: 0 
		[3 * tt + 0]: 0 
		[4 * tt + 0]: 0 
		[5 * tt + 0]: 0 
		[6 * tt + 0]: 0 
		[7 * tt + 0]: 0
		*/

		// 1
        int tt = tri * 3;			// 可以理解成是一个大面中的最后一个顶点，也就是越偏向X轴的点
        uv[0 * tt + 0].x = 0.875f;	
        uv[1 * tt + 0].x = 0.875f;	// 这里的索引表示，当前这个象限的第一个顶点。偏向Y轴两端的点
        uv[2 * tt + 0].x = 0.125f;
        uv[3 * tt + 0].x = 0.125f;
        uv[4 * tt + 0].x = 0.625f;
        uv[5 * tt + 0].x = 0.375f;
        uv[6 * tt + 0].x = 0.375f;
        uv[7 * tt + 0].x = 0.625f;

		// 1_1
		// 破案了，因为上下顶点，是很多个点浓缩成一个点(x = 0 , z = 0) 按照上面的公式，计算出来的u都是 == 0; 这是不合理，但是把上下8八点展开，重新手动赋值 uv 顶点
		// int tt = tri * 3;
        // uv[0 * tt + 0].x = 1f;
        // uv[1 * tt + 0].x = 1f;
        // uv[2 * tt + 0].x = 0f;
        // uv[3 * tt + 0].x = 0f;
        // uv[4 * tt + 0].x = 0.66f;
        // uv[5 * tt + 0].x = 0.33f;
        // uv[6 * tt + 0].x = 0.33f;
        // uv[7 * tt + 0].x = 0.66f;

    }

	// 4
	static void CreateColor(int level, Vector3[] vertices, Color[] color)
	{
		for (int i = 0; i < color.Length; ++i)
		{
			color[i] = Color.black;
		}

		int tri = level * level;
		int tt = tri * 3;			

        color[0 * tt + 0] = Color.red;//第一象限，上顶点
        color[1 * tt + 0] = Color.red;//第二象限，下顶点
        color[2 * tt + 0] = Color.red;//第三象限，上顶点
        color[3 * tt + 0] = Color.red;//第四象限，下顶点
        color[4 * tt + 0] = Color.red;//第五象限，上顶点
        color[5 * tt + 0] = Color.red;//第六象限，上顶点
        color[6 * tt + 0] = Color.red;//第七象限，下顶点
        color[7 * tt + 0] = Color.red;//第八象限，下顶点
	}

	// 5
    static void CreateTangents(Mesh mesh)
    {
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;
        Vector2[] uv = mesh.uv;
        Vector3[] normals = mesh.normals;

        int triangleCount = triangles.Length;
        int vertexCount = vertices.Length;

        Vector3[] tan1 = new Vector3[vertexCount];
        Vector3[] tan2 = new Vector3[vertexCount];

        Vector4[] tangents = new Vector4[vertexCount];

        for (int i = 0; i < triangleCount; i += 3)
        {
            int i1 = triangles[i + 0];
            int i2 = triangles[i + 1];
            int i3 = triangles[i + 2];

            Vector3 v1 = vertices[i1];
            Vector3 v2 = vertices[i2];
            Vector3 v3 = vertices[i3];

            Vector2 w1 = uv[i1];
            Vector2 w2 = uv[i2];
            Vector2 w3 = uv[i3];

            float x1 = v2.x - v1.x;
            float x2 = v3.x - v1.x;
            float y1 = v2.y - v1.y;
            float y2 = v3.y - v1.y;
            float z1 = v2.z - v1.z;
            float z2 = v3.z - v1.z;

            float s1 = w2.x - w1.x;
            float s2 = w3.x - w1.x;
            float t1 = w2.y - w1.y;
            float t2 = w3.y - w1.y;

            float r = 1.0f / (s1 * t2 - s2 * t1); // 这个是向量 叉乘 

            Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);// 这个公式，都看不懂
            Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

            tan1[i1] += sdir;
            tan1[i2] += sdir;
            tan1[i3] += sdir;

            tan2[i1] += tdir;
            tan2[i2] += tdir;
            tan2[i3] += tdir;
        }


        for (int i = 0; i < vertexCount; ++i)
        {
            Vector3 n = normals[i];
            Vector3 t = tan1[i];

            Vector3.OrthoNormalize(ref n, ref t);
            tangents[i].x = t.x;
            tangents[i].y = t.y;
            tangents[i].z = t.z;

            tangents[i].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0f) ? -1.0f : 1.0f;
        }

        mesh.tangents = tangents;
    }

}
