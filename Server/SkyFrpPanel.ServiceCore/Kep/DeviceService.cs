using JinianNet.JNTemplate.Nodes;
using Newtonsoft.Json.Linq;
using SkyFrpPanel.Infrastructure;
using SkyFrpPanel.Infrastructure.Attribute;
using SkyFrpPanel.Model;
using SkyFrpPanel.Model.Kep.Dto;
using SkyFrpPanel.Model.Kep.Model;
using SkyFrpPanel.Model.System;
using SkyFrpPanel.Repository;
using SqlSugar;
using System.Security.Cryptography;



namespace SkyFrpPanel.ServiceCore.Kep
{
    /// <summary>
    /// 设备管理
    /// </summary>
    [AppService(ServiceType = typeof(IDeviceService), ServiceLifetime = LifeTime.Transient)]
    public class DeviceService : BaseService<Device>, IDeviceService
    {
        private const string MASK = "********";

        /// <summary>
        /// 查询设备列表（分页），Token和Secret默认脱敏
        /// </summary>
        public PagedInfo<Device> SelectDeviceList(DeviceQueryDto dto)
        {
            var predicate = QueryExp(dto);
            var result = Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(it => it.Id)
                .ToPage(dto);

            foreach (var item in result.Result)
            {
                item.Token = string.IsNullOrEmpty(item.Token) ? "" : MASK;
                item.Secret = string.IsNullOrEmpty(item.Secret) ? "" : MASK;
            }
            return result;
        }

        /// <summary>
        /// 根据设备ID获取详细信息，Token和Secret脱敏
        /// </summary>
        public Device SelectDeviceById(long id)
        {
            var device = GetId(id);
            if (device != null)
            {
                device.Token = string.IsNullOrEmpty(device.Token) ? "" : MASK;
                device.Secret = string.IsNullOrEmpty(device.Secret) ? "" : MASK;
            }
            return device;
        }

        /// <summary>
        /// 根据设备ID获取Token明文
        /// </summary>
        public string SelectTokenById(long id)
        {
            var device = GetId(id);
            return device?.Token ?? "";
        }

        /// <summary>
        /// 根据设备ID获取Secret明文
        /// </summary>
        public string SelectSecretById(long id)
        {
            var device = GetId(id);
            return device?.Secret ?? "";
        }

        /// <summary>
        /// 新增设备
        /// </summary>
        public long InsertDevice(Device device)
        {
            if (UserConstants.NOT_UNIQUE.Equals(CheckSNUnique(device)))
                throw new CustomException($"添加设备'{device.SN}'失败，设备SN已存在");

            return InsertReturnBigIdentity(device);
        }

        /// <summary>
        /// 修改设备（Token和Secret为空时不更新对应字段，支持单独更新其中一个）
        /// </summary>
        public int UpdateDevice(Device device)
        {
            if (UserConstants.NOT_UNIQUE.Equals(CheckSNUnique(device)))
                throw new CustomException($"修改设备'{device.SN}'失败，设备SN已存在");

            bool tokenEmpty = string.IsNullOrEmpty(device.Token);
            bool secretEmpty = string.IsNullOrEmpty(device.Secret);

            // Token和Secret都为空 → 两个都不更新
            if (tokenEmpty && secretEmpty)
            {
                return Context.Updateable(device)
                    .IgnoreColumns(it => new { it.Token, it.Secret })
                    .ExecuteCommand();
            }
            // 只有Token为空 → 不更新Token，更新Secret
            if (tokenEmpty)
            {
                return Context.Updateable(device)
                    .IgnoreColumns(it => it.Token)
                    .ExecuteCommand();
            }
            // 只有Secret为空 → 不更新Secret，更新Token
            if (secretEmpty)
            {
                return Context.Updateable(device)
                    .IgnoreColumns(it => it.Secret)
                    .ExecuteCommand();
            }

            return Update(device);
        }

        /// <summary>
        /// 批量删除设备
        /// </summary>
        public int DeleteDeviceByIds(long[] ids)
        {
            return Delete(ids);
        }

        /// <summary>
        /// 校验设备SN是否唯一
        /// </summary>
        public string CheckSNUnique(Device device)
        {
            Device info = GetFirst(it => it.SN.Equals(device.SN));
            if (info != null && info.Id != device.Id)
                return UserConstants.NOT_UNIQUE;
            return UserConstants.UNIQUE;
        }

        /// <summary>
        /// 获取所有设备（导出用）
        /// </summary>
        public List<Device> SelectDeviceAll()
        {
            return Queryable().OrderByDescending(it => it.Id).ToList();
        }

        /// <summary>
        /// 设备认证
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public bool OnAuthentication(string SN,string Token)
        {
            return Context.Queryable<Device>()
            .Where(it => it.SN == SN && it.Token == Token)
            .Count()>0?true:false ;
        }

        /// <summary>
        /// 上线
        /// </summary>
        /// <param name="SN"></param>
        /// <returns></returns>
        public int Online(string SN)
        {
            //Context.Updateable(device)
            //.UpdateColumns(it => it.LastOnlineAt)  // 只更新这一列
            //.ExecuteCommand();
            return Context.Updateable<Device>()
                .SetColumns(it => it.LastOnlineAt == DateTime.Now)
            .Where(it => it.SN == SN)
            .ExecuteCommand();
        }

        /// <summary>
        /// 离线
        /// </summary>
        /// <param name="SN"></param>
        /// <returns></returns>
        public int Offline(string SN)
        {
            return Context.Updateable<Device>()
               .SetColumns(it => it.LastOfflineAt == DateTime.Now)
           .Where(it => it.SN == SN)
           .ExecuteCommand();
        }

        /// <summary>
        /// 构建查询条件
        /// </summary>
        private static Expressionable<Device> QueryExp(DeviceQueryDto dto)
        {
            var predicate = Expressionable.Create<Device>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.SN), it => it.SN.Contains(dto.SN));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.FirmwareVersion), it => it.FirmwareVersion.Contains(dto.FirmwareVersion));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.HardwareVersion), it => it.HardwareVersion.Contains(dto.HardwareVersion));
            return predicate;
        }

    }
}
