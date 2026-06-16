import { BaseResponse } from '../../../core/interfaces/base-response.interface';
import { ContactDto } from './contact-dto.interface';

export interface ContactListResponse extends BaseResponse<ContactDto[]> {}
