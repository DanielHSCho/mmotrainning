using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MultiplayersBuildAndRun
{
    static void PerformWin64Build(int playerCount)
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(
            BuildTargetGroup.Standalone,
            BuildTarget.StandaloneWindows);

        // 멀티 플레이어
        for(int i = 1; i <= playerCount; i++) {

            BuildPipeline.BuildPlayer(
                GetScenePaths(), // 씬 목록 가져옴
                $"Builds/Win64/{GetProjectName()}{i}/{GetProjectName()}{i}.exe", // 빌드 경로
                BuildTarget.StandaloneWindows64,
                BuildOptions.AutoRunPlayer); // 자동 실행 옵션

        }
    }

    // 프로젝트 이름 받기
    static string GetProjectName()
    {
        string[] s = Application.dataPath.Split('/');
        return s[s.Length - 2];
    }

    // 모든 씬에 대한 경로
    // 빌드세팅의 씬들 가져옴
    static string[] GetScenePaths()
    {
        string[] scenes = new string[EditorBuildSettings.scenes.Length];

        for(int i = 0; i < scenes.Length; i++) {
            scenes[i] = EditorBuildSettings.scenes[i].path;
        }

        return scenes;
    }
}
