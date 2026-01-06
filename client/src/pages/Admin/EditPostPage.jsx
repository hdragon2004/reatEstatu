import React, { useEffect, useState } from 'react';
import { Layout, Form, Input, Button, Upload, message, Spin, Select, InputNumber, Checkbox, DatePicker, Row, Col } from 'antd';
import dayjs from 'dayjs';
import { UploadOutlined } from '@ant-design/icons';
import Sidebar from '../../components/Sidebar';
import axiosPrivate from '../../api/axiosPrivate';
import { unwrapResponse } from '../../api/responseHelper';
import { useNavigate, useParams } from 'react-router-dom';

const { Content } = Layout;

const EditPostPage = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [post, setPost] = useState(null);
  const [existingImages, setExistingImages] = useState([]);
  const [selectedMainUrl, setSelectedMainUrl] = useState(null);
  // newMainFile: file chosen as new main image (single)
  const [newMainFile, setNewMainFile] = useState(null);
  // newOtherFiles: other images to upload (multiple)
  const [newOtherFiles, setNewOtherFiles] = useState([]);
  const [categories, setCategories] = useState([]);
  const [deletedImageIds, setDeletedImageIds] = useState([]);

  useEffect(() => {
    const fetchPost = async () => {
      try {
        const res = await axiosPrivate.get(`/api/posts/${id}`);
        const data = unwrapResponse(res);
        setPost(data);
        // Populate form with all known fields (support different casing)
        form.setFieldsValue({
          Title: data.title ?? data.Title,
          Description: data.description ?? data.Description,
          Price: data.price ?? data.Price,
          Area_Size: data.areaSize ?? data.area_Size ?? data.Area_Size,
          Street_Name: data.streetName ?? data.street_Name ?? data.Street_Name,
          CategoryId: data.categoryId ?? data.CategoryId ?? data.categoryId,
          TransactionType: (data.transactionType ?? data.TransactionType) || 'Sale',
          Status: data.status ?? data.Status,
          IsApproved: data.isApproved ?? data.IsApproved ?? false,
          ExpiryDate: data.expiryDate ? dayjs(data.expiryDate) : (data.ExpiryDate ? dayjs(data.ExpiryDate) : null),
          SoPhongNgu: data.soPhongNgu ?? data.SoPhongNgu ?? null,
          SoPhongTam: data.soPhongTam ?? data.SoPhongTam ?? null,
          SoTang: data.soTang ?? data.SoTang ?? null,
          HuongNha: data.huongNha ?? data.HuongNha ?? null,
          HuongBanCong: data.huongBanCong ?? data.HuongBanCong ?? null,
          MatTien: data.matTien ?? data.MatTien ?? null,
          DuongVao: data.duongVao ?? data.DuongVao ?? null,
          PhapLy: data.phapLy ?? data.PhapLy ?? null,
          FullAddress: data.fullAddress ?? data.FullAddress ?? null,
          Longitude: data.longitude ?? data.Longitude ?? null,
          Latitude: data.latitude ?? data.Latitude ?? null,
          PlaceId: data.placeId ?? data.PlaceId ?? null,
          CityName: data.cityName ?? data.CityName ?? null,
          DistrictName: data.districtName ?? data.DistrictName ?? null,
          WardName: data.wardName ?? data.WardName ?? null,
        });
        // Normalize image arrays from backend: support different property names and casings
        const rawImages = data.images ?? data.imageUrls ?? data.ImageUrls ?? data.Images ?? [];
        const normalized = (rawImages || []).map(img => ({
          id: img.id ?? img.Id,
          url: img.url ?? img.Url ?? img.path ?? img.Path ?? ''
        }));
        console.debug('Normalized images for post', normalized);
        setExistingImages(normalized);
        setSelectedMainUrl(data.imageURL);
      } catch (err) {
        message.error('Không thể tải bài viết.');
      } finally {
        setLoading(false);
      }
    };
    fetchPost();

    // fetch categories for dropdown (optional)
    const fetchCategories = async () => {
      try {
        const res = await axiosPrivate.get('/api/categories');
        const list = res.data?.data ?? res.data ?? [];
        setCategories(list);
      } catch (e) {
        // ignore
      }
    };
    fetchCategories();
  }, [id, form]);

  const handleMainUploadChange = ({ fileList }) => {
    // only keep the last selected (maxCount=1)
    const last = fileList.length ? fileList[fileList.length - 1].originFileObj : null;
    setNewMainFile(last || null);
  };

  const handleOtherUploadChange = ({ fileList }) => {
    const files = fileList.map(f => f.originFileObj).filter(Boolean);
    setNewOtherFiles(files);
  };

  const handleSubmit = async (values) => {
    setSaving(true);
    try {
      const formData = new FormData();
      formData.append('Id', id);
      formData.append('Title', values.Title);
      formData.append('Description', values.Description || '');
      formData.append('Price', values.Price?.toString() || '0');
      // Append all editable fields from form (use form.getFieldValue for fields not in values)
      const fv = form.getFieldsValue();
      const areaVal = fv.Area_Size ?? fv.areaSize ?? fv.Area_Size ?? 0;
      const streetVal = fv.Street_Name ?? fv.streetName ?? fv.Street_Name ?? '';
      const categoryVal = fv.CategoryId ?? fv.categoryId ?? (post?.categoryId ?? post?.CategoryId) ?? 0;
      const transactionVal = fv.TransactionType ?? fv.transactionType ?? (post?.transactionType ?? post?.TransactionType) ?? 'Sale';

      formData.append('Area_Size', areaVal?.toString() ?? '0');
      formData.append('Street_Name', streetVal?.toString() ?? '');
      formData.append('CategoryId', (categoryVal || 0).toString());
      const ttStr = String(transactionVal).toLowerCase().includes('rent') ? 'Rent' : 'Sale';
      formData.append('TransactionType', ttStr);

      // Other optional fields
      const optionalFields = [
        'Status', 'IsApproved', 'ExpiryDate', 'SoPhongNgu', 'SoPhongTam', 'SoTang',
        'HuongNha', 'HuongBanCong', 'MatTien', 'DuongVao', 'PhapLy',
        'FullAddress', 'Longitude', 'Latitude', 'PlaceId',
        'CityName', 'DistrictName', 'WardName'
      ];
      optionalFields.forEach((f) => {
        let val = fv[f] ?? fv[f.charAt(0).toLowerCase() + f.slice(1)] ?? post?.[f] ?? post?.[f.charAt(0).toLowerCase() + f.slice(1)];
        if (val !== undefined && val !== null) {
          // Special handling for ExpiryDate (dayjs or moment -> ISO)
          if (f === 'ExpiryDate') {
            if (val && (val._isAMomentObject || (typeof val.isValid === 'function' && typeof val.toISOString === 'function'))) {
              // moment or dayjs-like
              try {
                val = val.toISOString();
              } catch (e) {
                // fallback: convert via Date
                val = new Date(String(val)).toISOString();
              }
            } else if (typeof val === 'string') {
              // already string ISO
              val = val;
            } else {
              // unknown type, stringify
              val = String(val);
            }
          }
          formData.append(f, String(val));
        }
      });

      // If admin selected an existing image as main and did NOT upload a new main file,
      // send ImageURL so backend will use that existing image as primary.
      if (selectedMainUrl && newMainFile == null) {
        formData.append('ImageURL', selectedMainUrl);
      }

      // If admin uploaded a new main file, append it first so backend treats it as primary.
      if (newMainFile) {
        formData.append('Images', newMainFile, newMainFile.name);
      }

      // Append other new files
      for (const file of newOtherFiles) {
        formData.append('Images', file, file.name);
      }
      // Send deleted image ids if any so backend can remove them
      if (deletedImageIds.length > 0) {
        formData.append('DeletedImageIds', JSON.stringify(deletedImageIds));
      }

      await axiosPrivate.put(`/api/posts/${id}`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
      message.success('Cập nhật thành công');
      navigate('/admin/posts');
    } catch (err) {
      // Log backend response body when available for debugging (400 reasons)
      console.error('Update post error:', err.response?.data || err);
      const serverMessage = err.response?.data?.message || err.response?.data || null;
      message.error(serverMessage || 'Lỗi khi cập nhật bài viết');
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <Layout style={{ minHeight: '100vh' }}>
        <Sidebar selectedKey="/admin/posts" />
        <Layout>
          <Content style={{ padding: 24 }}>
            <Spin />
          </Content>
        </Layout>
      </Layout>
    );
  }

  // Determine current main image URL (selected existing, post.imageURL, or first existing image)
  const resolveImageUrl = (url) => {
    if (!url) return null;
    if (url.startsWith('http://') || url.startsWith('https://')) return url;
    const base = axiosPrivate.defaults.baseURL || '';
    // ensure single slash
    return `${base}${url.startsWith('/') ? '' : '/'}${url}`;
  };

  const rawMain = selectedMainUrl || (post && (post.imageURL ?? post.ImageURL)) || (existingImages.length ? existingImages[0].url : null);
  const currentMainUrl = resolveImageUrl(rawMain);
  // Other existing images (exclude main)
  const otherExistingImages = existingImages.filter(img => img.url !== rawMain);
  // Previews for new files
  const newMainPreviewUrl = newMainFile ? URL.createObjectURL(newMainFile) : null;
  const newOtherPreviews = newOtherFiles.map(f => ({ file: f, url: URL.createObjectURL(f) }));

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sidebar selectedKey="/admin/posts" />
      <Layout>
        <Content style={{ padding: 24 }}>
          <h1>Chỉnh sửa bài viết (Admin)</h1>
          <Form form={form} layout="vertical" onFinish={handleSubmit}>
            <Form.Item label="Tiêu đề" name="Title" rules={[{ required: true, message: 'Vui lòng nhập tiêu đề' }]}>
              <Input />
            </Form.Item>

            <Row gutter={12}>
              <Col span={12}>
                <Form.Item label="Giá" name="Price" rules={[{ required: true, message: 'Vui lòng nhập giá' }]}>
                  <InputNumber style={{ width: '100%' }} min={0} />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item label="Diện tích (m²)" name="Area_Size">
                  <InputNumber style={{ width: '100%' }} min={0} />
                </Form.Item>
              </Col>
            </Row>

            <Form.Item label="Mô tả" name="Description">
              <Input.TextArea rows={4} />
            </Form.Item>

            <Form.Item label="Đường / Địa chỉ" name="Street_Name">
              <Input />
            </Form.Item>

            <Row gutter={12}>
              <Col span={12}>
                <Form.Item label="Danh mục" name="CategoryId" rules={[{ required: true, message: 'Vui lòng chọn danh mục' }]}>
                  <Select loading={categories.length===0} options={categories.map(c => ({label: c.name, value: c.id}))} />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item label="Loại giao dịch" name="TransactionType">
                  <Select>
                    <Select.Option value="Sale">Sale</Select.Option>
                    <Select.Option value="Rent">Rent</Select.Option>
                  </Select>
                </Form.Item>
              </Col>
            </Row>

            <Row gutter={12}>
              <Col span={8}>
                <Form.Item label="Số phòng ngủ" name="SoPhongNgu">
                  <InputNumber style={{ width: '100%' }} min={0} />
                </Form.Item>
              </Col>
              <Col span={8}>
                <Form.Item label="Số phòng tắm" name="SoPhongTam">
                  <InputNumber style={{ width: '100%' }} min={0} />
                </Form.Item>
              </Col>
              <Col span={8}>
                <Form.Item label="Số tầng" name="SoTang">
                  <InputNumber style={{ width: '100%' }} min={0} />
                </Form.Item>
              </Col>
            </Row>

            <Row gutter={12}>
              <Col span={12}>
                <Form.Item label="Hướng nhà" name="HuongNha">
                  <Input />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item label="Hướng ban công" name="HuongBanCong">
                  <Input />
                </Form.Item>
              </Col>
            </Row>

            <Row gutter={12}>
              <Col span={12}>
                <Form.Item label="Mặt tiền (m)" name="MatTien">
                  <InputNumber style={{ width: '100%' }} min={0} />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item label="Đường vào (m)" name="DuongVao">
                  <InputNumber style={{ width: '100%' }} min={0} />
                </Form.Item>
              </Col>
            </Row>

            <Form.Item label="Pháp lý" name="PhapLy">
              <Input />
            </Form.Item>

            <Form.Item label="Địa chỉ đầy đủ" name="FullAddress">
              <Input />
            </Form.Item>

            <Row gutter={12}>
              <Col span={8}>
                <Form.Item label="Tọa độ Lat" name="Latitude">
                  <InputNumber style={{ width: '100%' }} />
                </Form.Item>
              </Col>
              <Col span={8}>
                <Form.Item label="Tọa độ Lng" name="Longitude">
                  <InputNumber style={{ width: '100%' }} />
                </Form.Item>
              </Col>
              <Col span={8}>
                <Form.Item label="PlaceId" name="PlaceId">
                  <Input />
                </Form.Item>
              </Col>
            </Row>

            <Row gutter={12}>
              <Col span={8}>
                <Form.Item label="Thành phố" name="CityName">
                  <Input />
                </Form.Item>
              </Col>
              <Col span={8}>
                <Form.Item label="Quận/Huyện" name="DistrictName">
                  <Input />
                </Form.Item>
              </Col>
              <Col span={8}>
                <Form.Item label="Phường/Xã" name="WardName">
                  <Input />
                </Form.Item>
              </Col>
            </Row>

            <Row gutter={12}>
              <Col span={8}>
                <Form.Item label="Trạng thái" name="Status">
                  <Select>
                    <Select.Option value="Pending">Pending</Select.Option>
                    <Select.Option value="Active">Active</Select.Option>
                    <Select.Option value="Rejected">Rejected</Select.Option>
                  </Select>
                </Form.Item>
              </Col>
              <Col span={8}>
                <Form.Item label="Đã duyệt" name="IsApproved" valuePropName="checked">
                  <Checkbox />
                </Form.Item>
              </Col>
              <Col span={8}>
                <Form.Item label="Ngày hết hạn" name="ExpiryDate">
                  <DatePicker showTime style={{ width: '100%' }} />
                </Form.Item>
              </Col>
            </Row>

            <Form.Item>
              <div style={{ display: 'flex', gap: 12, flexWrap: 'wrap' }}>
                {/* Show current main image prominently */}
                {currentMainUrl && (
                  <div style={{ marginBottom: 12 }}>
                    <div style={{ marginBottom: 6, fontWeight: 600 }}>Ảnh chính hiện tại</div>
                    <img
                      src={currentMainUrl}
                      alt="main"
                      style={{ width: 320, height: 200, objectFit: 'cover', border: '2px solid #1890ff', borderRadius: 6 }}
                    />
                  </div>
                )}
              </div>
            </Form.Item>

            {/* Show all remaining existing images as "Ảnh phụ hiện có" */}
            {existingImages.length > 0 && (
              <Form.Item label="Ảnh phụ hiện có">
                <div style={{ display: 'flex', gap: 12, flexWrap: 'wrap' }}>
                  {existingImages.map(img => {
                    const url = resolveImageUrl(img.url);
                    const isMain = rawMain === img.url;
                    return (
                      <div key={img.id} style={{ position: 'relative', textAlign: 'center' }}>
                        <a href={url} target="_blank" rel="noreferrer">
                          <img
                            src={url}
                            alt=""
                            style={{
                              width: 160,
                              height: 120,
                              objectFit: 'cover',
                              border: isMain ? '3px solid #1890ff' : '1px solid #ddd',
                              borderRadius: 6,
                            }}
                            onClick={() => setSelectedMainUrl(img.url)}
                          />
                        </a>
                        <div style={{ marginTop: 6, fontSize: 12 }}>
                          <span style={{ marginRight: 8 }}>{`#${img.id}`}</span>
                          {!isMain && (
                            <button
                              type="button"
                              onClick={() => {
                                setSelectedMainUrl(img.url);
                              }}
                              style={{
                                background: '#fff',
                                border: '1px solid #ddd',
                                borderRadius: 4,
                                padding: '2px 6px',
                                cursor: 'pointer'
                              }}
                            >
                              Chọn làm chính
                            </button>
                          )}
                        </div>
                      </div>
                    );
                  })}
                </div>
              </Form.Item>
            )}

            <Form.Item label="Upload ảnh chính mới (chỉ 1 ảnh)">
              <Upload beforeUpload={() => false} maxCount={1} onChange={handleMainUploadChange} accept="image/*">
                <Button icon={<UploadOutlined />}>Chọn ảnh chính</Button>
              </Upload>
              {newMainPreviewUrl && (
                <div style={{ marginTop: 8 }}>
                  <div style={{ marginBottom: 6, fontWeight: 600 }}>Preview ảnh chính mới</div>
                  <img src={newMainPreviewUrl} alt="new-main" style={{ width: 320, height: 200, objectFit: 'cover', borderRadius: 6 }} />
                </div>
              )}
            </Form.Item>

            <Form.Item label="Upload ảnh phụ mới (có thể nhiều ảnh)">
              <Upload beforeUpload={() => false} multiple onChange={handleOtherUploadChange} accept="image/*">
                <Button icon={<UploadOutlined />}>Chọn ảnh phụ</Button>
              </Upload>
              {newOtherPreviews.length > 0 && (
                <div style={{ marginTop: 8, display: 'flex', gap: 12, flexWrap: 'wrap' }}>
                  {newOtherPreviews.map((p, idx) => (
                    <div key={idx}>
                      <img src={p.url} alt={`other-${idx}`} style={{ width: 140, height: 100, objectFit: 'cover', borderRadius: 6 }} />
                    </div>
                  ))}
                </div>
              )}
            </Form.Item>

            <Form.Item>
              <Button type="primary" htmlType="submit" loading={saving}>Lưu thay đổi</Button>
              <Button style={{ marginLeft: 12 }} onClick={() => navigate('/admin/posts')}>Hủy</Button>
            </Form.Item>
          </Form>
        </Content>
      </Layout>
    </Layout>
  );
};

export default EditPostPage;


