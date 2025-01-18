using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace renameTwitterPictures
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                // 폴더 브라우저 대화상자의 설명 설정
                folderBrowserDialog.Description = "X 다운로드 이미지 파일이 들어있는 디렉토리를 선택하세요.";
                folderBrowserDialog.ShowNewFolderButton = true;

                // 다이얼로그 표시
                DialogResult result = folderBrowserDialog.ShowDialog();

                // 선택된 폴더 경로 사용
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
                {
                    string selectedPath = folderBrowserDialog.SelectedPath;
                    ProcessFiles(selectedPath);

                    /*
                     * selectedPath 안에는 "HlsDeveloper-1880607867230900444-20250118222710-01" 식으로 된 파일들이 있으며 확장자는 아무거나 올 수 있어.
                     * 하이픈 기준으로 나누면 1번째 문자열은 Account, 2번째 TweetID, 3번째 문자열은 DateTime(YYYYMMDDhhmmss), 4번째 문자열은 SequenceNumber야.
                     * 내가 원하는 코드는 동일한 Account는 전부 "@Account" 디렉토리 안에 넣되,
                     * 파일명 오름차순대로 이름을 전부 바꿀거야. 이름은 "YYYYMMDD_hhmmss-SequenceNumber.확장자"이어야 돼.
                     * 여기서 YYYY는 연도, MM은 월, DD는 날짜이며 hhmmss는 시간-분-초이며 끝의 2자리 숫자는 순번이야.
                     */
                }
            }
        }

        private static void ProcessFiles(string path)
        {
            // 파일 목록을 가져옵니다.
            var files = Directory.GetFiles(path);

            // Account별로 파일들을 그룹화합니다.
            var groupedFiles = files
                .Select(file => new
                {
                    FilePath = file,
                    FileName = Path.GetFileName(file),
                    Parts = Path.GetFileName(file).Split('-')
                })
                .Where(file => file.Parts.Length == 4)  // 파일명이 4개의 부분으로 나뉘어야 함 (Account, TweetID, DateTime, SequenceNumber)
                .GroupBy(file => file.Parts[0]);    // // Account별로 그룹화

            foreach (var group in groupedFiles)
            {
                string account = group.Key;
                string accountDirectory = Path.Combine(path, $"@{account}");

                // Account별 디렉토리를 생성합니다.
                if (!Directory.Exists(accountDirectory))
                {
                    Directory.CreateDirectory(accountDirectory);
                }

                // 파일명을 오름차순으로 정렬합니다.
                var sortedFiles = group.OrderBy(file => file.FileName).ToList();
                int counter = 1;

                foreach (var file in sortedFiles)
                {
                    string originalPath = file.FilePath;
                    string extension = Path.GetExtension(originalPath);
                    string tweetDateTime = file.Parts[2]; // 3번째 부분이 DateTime(YYYYMMDDhhmmss)
                    string sequenceNumber = file.Parts[3]; // 4번째 부분이 SequenceNumber

                    // DateTime을 다시 포맷팅 (YYYYMMDD_hhmmss)
                    string formattedDateTime = tweetDateTime.Insert(8, "_");

                    // 새 파일명 생성: "YYYYMMDD_hhmmss-SequenceNumber.확장자"
                    string newFileName = $"{formattedDateTime}-{sequenceNumber}";
                    string newFilePath = Path.Combine(accountDirectory, newFileName);

                    // 파일을 새 이름으로 이동합니다.
                    File.Move(originalPath, newFilePath);
                    counter++;
                }
            }

            MessageBox.Show("처리 완료");
        }
    }
}
