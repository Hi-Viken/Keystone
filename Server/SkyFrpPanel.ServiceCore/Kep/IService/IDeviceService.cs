using JinianNet.JNTemplate;
using SkyFrpPanel.Model;
using SkyFrpPanel.Model.Kep.Dto;
using SkyFrpPanel.Model.Kep.Model;

namespace SkyFrpPanel.ServiceCore.Kep
{
    public interface IDeviceService : IBaseService<Device>
    {
        /// <summary>
        /// 查询设备列表（分页），敏感字段脱敏
        /// </summary>
        PagedInfo<Device> SelectDeviceList(DeviceQueryDto dto);

        /// <summary>
        /// 根据设备ID获取详细信息，敏感字段脱敏
        /// </summary>
        Device SelectDeviceById(long id);

        /// <summary>
        /// 根据设备ID获取Token明文
        /// </summary>
        string SelectTokenById(long id);

        /// <summary>
        /// 根据设备ID获取Secret明文
        /// </summary>
        string SelectSecretById(long id);

        /// <summary>
        /// 新增设备
        /// </summary>
        long InsertDevice(Device device);

        /// <summary>
        /// 修改设备
        /// </summary>
        int UpdateDevice(Device device);

        /// <summary>
        /// 批量删除设备
        /// </summary>
        int DeleteDeviceByIds(long[] ids);

        /// <summary>
        /// 校验设备SN是否唯一
        /// </summary>
        string CheckSNUnique(Device device);

        /// <summary>
        /// 获取所有设备（导出用）
        /// </summary>
        List<Device> SelectDeviceAll();

        /// <summary>
        /// 设备认证
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public bool OnAuthentication(string SN, string Token);

        /// <summary>
        /// 上线
        /// </summary>
        /// <param name="SN"></param>
        /// <returns></returns>
        int Online(string SN);

        /// <summary>
        /// 离线
        /// </summary>
        /// <param name="SN"></param>
        /// <returns></returns>
        int Offline(string SN);
    }
}
