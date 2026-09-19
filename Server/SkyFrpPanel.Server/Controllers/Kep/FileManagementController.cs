using Microsoft.AspNetCore.Mvc;
using SkyFrpPanel.Infrastructure;
using SkyFrpPanel.Model.Kep.Dto;
using SkyFrpPanel.Model.Kep.Model;
using SkyFrpPanel.ServiceCore.Kep;
using System.Security.Cryptography;

namespace SkyFrpPanel.Server.Controllers.Kep
{
    /// <summary>
    /// 文件管理
    /// </summary>
    [Route("FileManagement/[action]")]
    public class FileManagementController : BaseController
    {
        private readonly IFileManagementService FileManagementService;
        private readonly IWebHostEnvironment WebHostEnvironment;

        public FileManagementController(IFileManagementService fileManagementService, IWebHostEnvironment webHostEnvironment)
        {
            FileManagementService = fileManagementService;
            WebHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// 文件管理列表查询（分页）
        /// </summary>
        [HttpGet]
        [ActionPermissionFilter(Permission = "kep:filemanagement:list")]
        public IActionResult List([FromQuery] FileManagementQueryDto dto)
        {
            var list = FileManagementService.SelectFileManagementList(dto);
            return SUCCESS(list);
        }

        /// <summary>
        /// 按父级ID分页查询当前目录内容
        /// </summary>
        [HttpGet]
        [ActionPermissionFilter(Permission = "kep:filemanagement:list")]
        public IActionResult ListByParentId([FromQuery] FileManagementQueryDto dto)
        {
            var list = FileManagementService.SelectFileManagementByParentId(dto);
            return SUCCESS(list);
        }

        /// <summary>
        /// 获取面包屑路径
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "kep:filemanagement:list")]
        public IActionResult Breadcrumb(long id = 0)
        {
            var path = FileManagementService.GetBreadcrumbPath(id);
            return SUCCESS(path);
        }

        /// <summary>
        /// 文件管理树形列表查询
        /// </summary>
        [HttpGet]
        [ActionPermissionFilter(Permission = "kep:filemanagement:list")]
        public IActionResult TreeList([FromQuery] FileManagementQueryDto dto)
        {
            var list = FileManagementService.SelectFileManagementTreeList(dto);
            return SUCCESS(list);
        }

        /// <summary>
        /// 文件管理列表（排除指定节点及其子节点）
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "kep:filemanagement:list")]
        public IActionResult ExcludeChild(long id = 0)
        {
            var list = FileManagementService.SelectFileManagementListExcludeChild(id);
            return SUCCESS(list);
        }

        /// <summary>
        /// 根据ID获取详细信息
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "kep:filemanagement:query")]
        public IActionResult Query(long id = 0)
        {
            return SUCCESS(FileManagementService.SelectFileManagementById(id));
        }

        /// <summary>
        /// 添加文件/目录
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "kep:filemanagement:add")]
        [Log(Title = "文件管理添加", BusinessType = BusinessType.INSERT)]
        public IActionResult Add([FromBody] FileManagement fileManagement)
        {
            // 检查同级目录/文件名是否重复
            if (FileManagementService.CheckDuplicateName(fileManagement.ParentId, fileManagement.FileName))
            {
                throw new CustomException(ResultCode.PARAM_ERROR, "同级目录下已存在相同名称");
            }
            
            fileManagement.ToCreate(HttpContext);
            return ToResponse(FileManagementService.InsertFileManagement(fileManagement));
        }

        /// <summary>
        /// 修改文件/目录
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "kep:filemanagement:edit")]
        [Log(Title = "文件管理编辑", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] FileManagement fileManagement)
        {
            // 检查同级目录/文件名是否重复（排除自身）
            if (FileManagementService.CheckDuplicateName(fileManagement.ParentId, fileManagement.FileName, fileManagement.Id))
            {
                throw new CustomException(ResultCode.PARAM_ERROR, "同级目录下已存在相同名称");
            }
            
            fileManagement.ToUpdate(HttpContext);
            return ToResponse(FileManagementService.UpdateFileManagement(fileManagement));
        }

        /// <summary>
        /// 删除文件/目录
        /// </summary>
        [HttpDelete("{id}")]
        [ActionPermissionFilter(Permission = "kep:filemanagement:remove")]
        [Log(Title = "文件管理删除", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string id)
        {
            long[] ids = Tools.SpitLongArrary(id);
            return ToResponse(FileManagementService.DeleteFileManagementByIds(ids));
        }

        /// <summary>
        /// 文件管理导出
        /// </summary>
        [Log(BusinessType = BusinessType.EXPORT, IsSaveResponseData = false, Title = "文件管理导出")]
        [HttpGet()]
        [ActionPermissionFilter(Permission = "kep:filemanagement:export")]
        public IActionResult Export()
        {
            var list = FileManagementService.SelectFileManagementAll();
            var result = ExportExcelMini(list, "filemanagement", "文件管理列表");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "kep:filemanagement:add")]
        [Log(Title = "文件上传", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> UploadFile([FromForm] long parentId, IFormFile file)
        {
            if (file == null) throw new CustomException(ResultCode.PARAM_ERROR, "上传文件不能为空");

            string originalFileName = file.FileName;
            
            // 检查同级目录/文件名是否重复
            if (FileManagementService.CheckDuplicateName(parentId, originalFileName))
            {
                throw new CustomException(ResultCode.PARAM_ERROR, "同级目录下已存在相同名称的文件");
            }
            
            string fileExt = Path.GetExtension(originalFileName);
            long fileSize = file.Length;

            // 生成随机文件名（GUID）
            string randomFileName = Guid.NewGuid().ToString("N") + fileExt;

            // 获取存储根目录
            string rootPath = Path.Combine(WebHostEnvironment.WebRootPath, "uploads", "filemanagement");

            // 根据 parentId 构建存储路径
            string relativePath = BuildStoragePath(parentId);
            string saveDir = Path.Combine(rootPath, relativePath);

            if (!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
            }

            // 保存文件并计算哈希
            string savePath = Path.Combine(saveDir, randomFileName);
            string fileHash;
            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;
                using (var sha256 = SHA256.Create())
                {
                    var hashBytes = await sha256.ComputeHashAsync(stream);
                    fileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                }
            }

            // 构建访问路径
            string accessPath = $"/uploads/filemanagement/{relativePath.Replace("\\", "/")}/{randomFileName}";

            // 保存到数据库（FileName保存原文件名）
            var fileRecord = new FileManagement
            {
                ParentId = parentId,
                FileType = "1",
                FileName = originalFileName,
                FilePath = accessPath,
                FileSize = fileSize,
                FileExtension = fileExt.TrimStart('.'),
                FileHash = fileHash,
                Create_by = HttpContext.GetName(),
                Create_time = DateTime.Now
            };

            var id = FileManagementService.InsertFileManagement(fileRecord);
            return SUCCESS(new { id, filePath = accessPath });
        }

        /// <summary>
        /// 批量上传文件
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "kep:filemanagement:add")]
        [Log(Title = "批量文件上传", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> UploadFiles([FromForm] long parentId, List<IFormFile> files)
        {
            if (files == null || files.Count == 0) throw new CustomException(ResultCode.PARAM_ERROR, "上传文件不能为空");

            string rootPath = Path.Combine(WebHostEnvironment.WebRootPath, "uploads", "filemanagement");
            string relativePath = BuildStoragePath(parentId);
            string saveDir = Path.Combine(rootPath, relativePath);

            if (!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
            }

            int successCount = 0;
            List<long> fileIds = new List<long>();
            
            foreach (var file in files)
            {
                if (file == null || file.Length == 0) continue;

                string originalFileName = file.FileName;
                
                // 检查同级目录/文件名是否重复
                if (FileManagementService.CheckDuplicateName(parentId, originalFileName))
                {
                    throw new CustomException(ResultCode.PARAM_ERROR, $"同级目录下已存在相同名称的文件：{originalFileName}");
                }
                
                string fileExt = Path.GetExtension(originalFileName);
                
                // 生成随机文件名（GUID）
                string randomFileName = Guid.NewGuid().ToString("N") + fileExt;
                string savePath = Path.Combine(saveDir, randomFileName);

                // 保存文件并计算哈希
                string fileHash;
                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;
                    using (var sha256 = SHA256.Create())
                    {
                        var hashBytes = await sha256.ComputeHashAsync(stream);
                        fileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                    }
                }

                string accessPath = $"/uploads/filemanagement/{relativePath.Replace("\\", "/")}/{randomFileName}";

                var fileRecord = new FileManagement
                {
                    ParentId = parentId,
                    FileType = "1",
                    FileName = originalFileName,
                    FilePath = accessPath,
                    FileSize = file.Length,
                    FileExtension = fileExt.TrimStart('.'),
                    FileHash = fileHash,
                    Create_by = HttpContext.GetName(),
                    Create_time = DateTime.Now
                };

                var fileId = FileManagementService.InsertFileManagement(fileRecord);
                fileIds.Add(fileId);
                successCount++;
            }

            return SUCCESS(new { successCount, totalCount = files.Count, fileIds });
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public IActionResult Download(long id)
        {
            var fileRecord = FileManagementService.SelectFileManagementById(id);
            if (fileRecord == null || fileRecord.FileType != "1")
            {
                return ToResponse(ResultCode.PARAM_ERROR, "文件不存在或不是文件类型");
            }

            // FilePath 存储的是完整相对路径，如 /uploads/filemanagement/xxx.jpg
            string physicalPath = Path.Combine(WebHostEnvironment.WebRootPath, fileRecord.FilePath.TrimStart('/'));

            if (!System.IO.File.Exists(physicalPath))
            {
                return ToResponse(ResultCode.PARAM_ERROR, "物理文件不存在");
            }

            var fileBytes = System.IO.File.ReadAllBytes(physicalPath);
            
            // 根据文件扩展名确定MIME类型
            var contentType = GetContentType(fileRecord.FileExtension);
            
            return File(fileBytes, contentType);
        }

        /// <summary>
        /// 根据文件扩展名获取MIME类型
        /// </summary>
        private string GetContentType(string extension)
        {
            var ext = extension.ToLower().TrimStart('.');
            return ext switch
            {
                // 图片
                "jpg" or "jpeg" => "image/jpeg",
                "png" => "image/png",
                "gif" => "image/gif",
                "bmp" => "image/bmp",
                "webp" => "image/webp",
                "svg" => "image/svg+xml",
                // 视频
                "mp4" => "video/mp4",
                "avi" => "video/x-msvideo",
                "mov" => "video/quicktime",
                "wmv" => "video/x-ms-wmv",
                "flv" => "video/x-flv",
                "mkv" => "video/x-matroska",
                // 音频
                "mp3" => "audio/mpeg",
                "wav" => "audio/wav",
                "ogg" => "audio/ogg",
                "aac" => "audio/aac",
                // 文档
                "pdf" => "application/pdf",
                "doc" => "application/msword",
                "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "xls" => "application/vnd.ms-excel",
                "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "ppt" => "application/vnd.ms-powerpoint",
                "pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                // 压缩包
                "zip" => "application/zip",
                "rar" => "application/x-rar-compressed",
                "7z" => "application/x-7z-compressed",
                // 默认
                _ => "application/octet-stream"
            };
        }

        /// <summary>
        /// 根据 parentId 构建存储路径
        /// </summary>
        private string BuildStoragePath(long parentId)
        {
            if (parentId == 0) return "";

            var pathParts = new List<string>();
            long currentId = parentId;

            while (currentId != 0)
            {
                var node = FileManagementService.SelectFileManagementById(currentId);
                if (node == null) break;
                pathParts.Insert(0, node.FileName);
                currentId = node.ParentId;
            }

            return string.Join("/", pathParts);
        }
    }
}
